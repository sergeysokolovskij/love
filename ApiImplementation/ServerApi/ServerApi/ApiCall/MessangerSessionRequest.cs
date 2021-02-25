using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServerApi.Dal;
using ServerApi.Models.Request;
using ServerApi.Models.Response;
using ServerApi.Options;
using ServerApi.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ServerApi.ApiCall
{
	public interface IMessangerSessionRequest
	{
		Task SetServerSessionAsync();
	}
	public class MessangerSessionRequest : IMessangerSessionRequest
	{
		private readonly IAuthRequest authRequest;
		private readonly IOptions<UrlOptions> urlOptions;
		private readonly ApplicationContext db;

		private readonly IAesCipher aes;

		public MessangerSessionRequest(IAuthRequest authRequest,
			IOptions<UrlOptions> urlOptions,
			ApplicationContext db,
			IAesCipher aes)
		{
			this.authRequest = authRequest;
			this.urlOptions = urlOptions;
			this.db = db;
			this.aes = aes;
		}

		public async Task SetServerSessionAsync()
		{
			var strongKey = await db.StrongKeys.SingleOrDefaultAsync();
			var session = await db.Sessions.SingleOrDefaultAsync();

			string cryptedSession = await aes.Crypt(session.SessionId);

			await authRequest.MakeHttpRequestWithTextResultAsync(urlOptions.Value.SetServerSessionUrl + cryptedSession, HttpMethod.Get, null);
		}
	}
}
