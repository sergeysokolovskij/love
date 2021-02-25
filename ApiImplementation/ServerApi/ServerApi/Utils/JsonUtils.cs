using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerApi.Utils
{
	public static class JsonUtils
	{
		public static async Task<string> ToJson(this object obj)
		{
			using (var memStream = new MemoryStream())
			{
				await JsonSerializer.SerializeAsync(memStream, obj, obj.GetType());
				memStream.Position = 0;

				using (var streamReader = new StreamReader(memStream))
				{
					return await streamReader.ReadToEndAsync();
				}
			}
		}

		public static async Task<T> FromJson<T>(this string str)
		{
			using (var memStream = new MemoryStream())
			{
				byte[] buffer = Encoding.UTF8.GetBytes(str);
				await memStream.WriteAsync(buffer, 0, buffer.Length);
				memStream.Position = 0;

				var result = await JsonSerializer.DeserializeAsync<T>(memStream);

				return result;
			}
		}


		public static async Task<T> FromJson<T>(this Stream stream)
		{
			using (var streamReader = new StreamReader(stream))
			{
				var result = await JsonSerializer.DeserializeAsync<T>(stream);
				return result;
			}
		}
	}
}
