using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServerApi.Dal;
using ServerApi.Models;
using ServerApi.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ServerApi.Utils;
using Microsoft.Extensions.Configuration;

namespace ServerApi.RealTime
{
	public interface IMessageHubService
	{
		Task ReceiveMessageAsync(string message);
	}
	public class MessageHubService : IMessageHubService
	{
		private readonly IAesCipher aes;
		private readonly IRsaCryptService rsa;

		private readonly ApplicationContext db;

		private readonly IBrockerService brockerService;
		private readonly IConfiguration configuration;

		public MessageHubService(IAesCipher aes,
			IRsaCryptService rsa,
			ApplicationContext db,
			IBrockerService brockerService,
			IConfiguration configuration)
		{
			this.aes = aes;
			this.db = db;
			this.rsa = rsa;
			this.brockerService = brockerService;
			this.configuration = configuration;
		}

		public async Task ReceiveMessageAsync(string message)
		{
			try
			{
				var model = JsonConvert.DeserializeObject<MessageDto>(message);

				var session = await db.Sessions.SingleOrDefaultAsync();
				string decryptedAes = await aes.Decrypt(model.Message.CryptedAes);
				string decryptedText = aes.Decrypt(decryptedAes, model.Message.CryptedText);	
				Console.WriteLine($"Received message: {decryptedText}");

				var readMessageModel = new
				{
					userId = configuration.GetValue<string>("UserId"),
					messageId = await aes.Crypt(model.Message.MessageId)
				};

				string modelToSend = JsonConvert.SerializeObject(readMessageModel);
				await brockerService.PuplishReadMessage(modelToSend);

				if (rsa.VerifySignature(session.ServerPublicKey, model.Message.ObjectToBytes(), model.Sign.FromUrlSafeBase64()))
				{
					Console.WriteLine("Signature was verified");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
		}
	}
}
