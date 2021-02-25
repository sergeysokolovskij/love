using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ServerApi.Utils
{
	public static class CommonUtils
	{
		public static byte[] ObjectToBytes(this object obj)
		{
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream memStream = new MemoryStream())
			{
				bf.Serialize(memStream, obj);
				byte[] result = memStream.ToArray();
				return result;
			}
		}
	}
}
