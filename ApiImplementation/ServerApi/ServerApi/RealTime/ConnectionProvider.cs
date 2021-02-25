using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServerApi.Dal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerApi.RealTime
{
	public interface IConnectionProvider
	{
		Task<HubConnection> MakeHubConnectionAsync();
	}
	public class ConnectionProvider : IConnectionProvider
	{
		private readonly ApplicationContext Context;
		private readonly IAuthRequest authRequest;

		public ConnectionProvider(ApplicationContext context,
			IAuthRequest authRequest)
		{
			Context = context;
			this.authRequest = authRequest;
		}

		public async Task<HubConnection> MakeHubConnectionAsync()
		{
			var authResult = await authRequest.GetAuthTokenAsync();
			return new HubConnectionBuilder()
				.WithUrl("https://localhost:5001/messanger", options =>
				{
					options.AccessTokenProvider = () => Task.FromResult(authResult.AccessToken);
				})
				.WithAutomaticReconnect()
				.AddMessagePackProtocol()
				.Build();
		}
	}
}
