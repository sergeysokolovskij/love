using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServerApi.Dal;
using ServerApi.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ServerApi.Utils;
using System.Security.Cryptography;

namespace ServerApi.Services
{
	public interface IMessangerService
	{
		Task<string> GetMessageToSendAsync(string message);
	}
	public class MessangerService : IMessangerService
	{
		private readonly ApplicationContext db;
		private readonly IAesCipher aes;
		private readonly IRsaCryptService rsa;
		private readonly IConfiguration configuration;

		public MessangerService(ApplicationContext db,
			IAesCipher aes,
			IRsaCryptService rsa,
			IConfiguration configuration)
		{
			this.db = db;
			this.aes = aes;
			this.configuration = configuration;
			this.rsa = rsa;
		}

		public async Task<string> GetMessageToSendAsync(string message)
		{
			string aesKey = aes.GenerateAesKey();

			string cryptedAesKey = await aes.Crypt(aesKey);
			string cryptedMessage = aes.Crypt(aesKey, message);
			var session = await db.Sessions.SingleOrDefaultAsync();

			var signMessage = new SignMessageDto()
			{
				MessageId = Guid.NewGuid().ToString(),
				Created = DateTime.Now,
				CryptedAes = cryptedAesKey,
				CryptedText = cryptedMessage,
				SessionId = session.SessionId,
				SenderId = configuration.GetValue<string>("UserId"),
				ReceiverId = configuration.GetValue<string>("ReceiverId")
			};
			var messageDto = new MessageDto()
			{
				Message = signMessage,
				Sign = rsa.SignData(session.ClientPrivateKey, signMessage.ObjectToBytes())
			};

			return JsonConvert.SerializeObject(messageDto);
		}
	}
}
