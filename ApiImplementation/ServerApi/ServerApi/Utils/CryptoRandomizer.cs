﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ServerApi.Utils
{
	public static class CryptoRandomizer
	{
		public static readonly RNGCryptoServiceProvider CryptoProvider = new RNGCryptoServiceProvider();

		const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

		public static string GetRandomString(int length)
		{
			StringBuilder res = new StringBuilder();
			byte[] uintBuffer = new byte[sizeof(uint)];

			while (length-- > 0)
			{
				CryptoProvider.GetBytes(uintBuffer);
				uint num = BitConverter.ToUInt32(uintBuffer, 0);
				res.Append(valid[(int)(num % (uint)valid.Length)]);
			}

			return res.ToString();
		}

		public static uint GetRandomUint()
		{
			byte[] uintBuffer = new byte[sizeof(uint)];
			CryptoProvider.GetBytes(uintBuffer);
			return BitConverter.ToUInt32(uintBuffer, 0);
		}

		public static int GetRandomInt(int min, int max)
		{
			uint r = GetRandomUint();
			return (int)(r % (max - min + 1)) + min;
		}

		public static byte[] GenerateSecurityKey(int countBytes)
		{
			byte[] result = new byte[countBytes];
			CryptoProvider.GetBytes(result);
			return result;
		}
	}
}
