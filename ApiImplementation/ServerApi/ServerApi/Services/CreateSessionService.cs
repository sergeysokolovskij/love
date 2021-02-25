using Microsoft.EntityFrameworkCore;
using ServerApi.ApiCall;
using ServerApi.Dal;
using ServerApi.Models.Request;
using ServerApi.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerApi.Services
{
	public interface ICreateSessionService
	{
	}
	public class CreateSessionService : ICreateSessionService
	{
		private readonly ApplicationContext db;

		private readonly IRsaCryptService rsaCryptService;
		private readonly IAesCipher aes;

		private readonly IAuthRequest authRequest;
		private readonly IMessangerSessionRequest createMessangerSessionRequest;

		public CreateSessionService(ApplicationContext db,
			IRsaCryptService rsaCryptService,
			IAesCipher aes,
			IAuthRequest authRequest,
			IMessangerSessionRequest createMessangerSessionRequest)
		{
			this.db = db;
			this.rsaCryptService = rsaCryptService;
			this.aes = aes;
			this.createMessangerSessionRequest = createMessangerSessionRequest;
			this.authRequest = authRequest;
		}

		//public async Task CreateSessionAsync()
		//{
		//	var rsaPair = rsaCryptService.GenerateKeys();
		//	var strongKeys = await db.StrongKeys.SingleOrDefaultAsync();

		//	if (strongKeys == null)
		//	{
		//		var response = await createMessangerSessionRequest.CreateFirstSessionAsync(rsaPair.publicKey);

		//		byte[] decryptedAesKey = rsaCryptService.Decrypt(rsaPair.privateKey, response.CryptedAes).FromUrlSafeBase64(); //на этом этапе имеем расшифрованый ключ aes для работы

		//		db.StrongKeys.Add(new StrongKey()
		//		{
		//			Cypher = decryptedAesKey
		//		});

		//		await db.SaveChangesAsync();

		//		await authRequest.GetAuthTokenAsync(true);

		//		rsaPair = rsaCryptService.GenerateKeys();
		//		string cryptedPublicKey = await aes.Crypt(rsaPair.publicKey);

		//		var sessionResponse = await createMessangerSessionRequest.CreateSessionAsync(cryptedPublicKey);

		//		string decryptedServerPublicKey = await aes.DecryptString(sessionResponse.ServerPublicKey);

		//		db.Sessions.Add(new Session()
		//		{
		//			ClientPublicKey = rsaPair.publicKey,
		//			ClientPrivateKey = rsaPair.privateKey,
		//			ServerPublicKey = decryptedServerPublicKey,
		//			Created = DateTime.Now
		//		});
		//	}
		//	else
		//	{
		//		await authRequest.GetAuthTokenAsync(true);

		//		string cryptedPublicKey = await aes.Crypt(rsaPair.publicKey);

		//		var response = await createMessangerSessionRequest.CreateSessionAsync(cryptedPublicKey);
		//		var session = await db.Sessions.SingleOrDefaultAsync();

		//		string decryptedServerPublicKey = await aes.DecryptString(response.ServerPublicKey);
		//		string decryptedSessionId = await aes.DecryptString(response.SessionId);

		//		session.ServerPublicKey = decryptedServerPublicKey;
		//		session.ClientPrivateKey = rsaPair.privateKey;
		//		session.ClientPublicKey = rsaPair.publicKey;
		//		session.SessionId = decryptedSessionId;

		//		db.Entry(session).State = EntityState.Modified;
		//	}
		//	await db.SaveChangesAsync();
		//}
	}
}
