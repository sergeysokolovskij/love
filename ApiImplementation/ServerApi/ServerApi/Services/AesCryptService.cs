using Microsoft.EntityFrameworkCore;
using ServerApi.Dal;
using ServerApi.Utils;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServerApi.Services
{
	public interface IAesCipher
	{
		string Crypt(string secret, string value);
		string Decrypt(string secret, string value);
		Task<string> Crypt(string value);
		Task<string> Decrypt(string value);
		string GenerateAesKey();
	}
	public class AesCryptService : IAesCipher
	{
		private readonly ApplicationContext db;

		Aes cipher;

		public AesCryptService(ApplicationContext db)
		{
			this.db = db;
			cipher = new AesManaged
			{
				KeySize = 256,
				BlockSize = 128,
				Padding = PaddingMode.ISO10126,
				Mode = CipherMode.CBC
			};
		}

		public async Task<string> Crypt(string value)
		{
			var cypherSecret = await db.StrongKeys.SingleOrDefaultAsync();

			var IV = GenerateIV();

			var cryptoTransform = cipher.CreateEncryptor(cypherSecret.Cypher, IV);
			byte[] textInBytes = Encoding.UTF8.GetBytes(value);
			byte[] result = cryptoTransform.TransformFinalBlock(textInBytes, 0, textInBytes.Length);

			byte[] resultPlusIV = new byte[result.Length + IV.Length];

			result.CopyTo(resultPlusIV, 0);
			IV.CopyTo(resultPlusIV, result.Length);

			return resultPlusIV.ToUrlSafeBase64();
		}

		public async Task<string> Decrypt(string value)
		{
			var cypherSecret = await db.StrongKeys.SingleOrDefaultAsync();

			byte[] textInBytes = value.FromUrlSafeBase64();
			byte[] IV = new byte[16];

			Array.Copy(textInBytes, textInBytes.Length - IV.Length, IV, 0, IV.Length);

			var cryptoTransform = cipher.CreateDecryptor(cypherSecret.Cypher, IV);

			byte[] result = cryptoTransform.TransformFinalBlock(textInBytes, 0, textInBytes.Length - IV.Length);

			return Encoding.UTF8.GetString(result);
		}
		public string Crypt(string secret, string value)
		{
			byte[] secretBuffer = secret.FromUrlSafeBase64();
			byte[] IV = GenerateIV();

			var cryptoTransform = cipher.CreateEncryptor(secretBuffer, IV);
			byte[] textInBytes = Encoding.UTF8.GetBytes(value);

			byte[] result = cryptoTransform.TransformFinalBlock(textInBytes, 0, textInBytes.Length);
			byte[] resultPlusIV = new byte[result.Length + IV.Length];

			result.CopyTo(resultPlusIV, 0);
			IV.CopyTo(resultPlusIV, result.Length);

			return resultPlusIV.ToUrlSafeBase64();
		}

		public string Decrypt(string secret, string value)
		{
			byte[] secretBuffer = secret.FromUrlSafeBase64();
			byte[] IV = new byte[16];
			byte[] textInBytes = value.FromUrlSafeBase64();

			Array.Copy(textInBytes, textInBytes.Length - IV.Length, IV, 0, IV.Length);
			var cryptoTransform = cipher.CreateDecryptor(secretBuffer, IV);

			byte[] result = cryptoTransform.TransformFinalBlock(textInBytes, 0, textInBytes.Length - IV.Length);
			return Encoding.UTF8.GetString(result);
		}
		public string GenerateAesKey()
		{
			var result = CryptoRandomizer.GenerateSecurityKey(32);
			return result.ToUrlSafeBase64();
		}
		private byte[] GenerateIV()
		{
			var result = new byte[16];
			CryptoRandomizer.CryptoProvider.GetBytes(result);
			return result;
		}
	}
}
