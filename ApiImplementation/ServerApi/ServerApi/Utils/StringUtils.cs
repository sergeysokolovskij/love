using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ServerApi.Utils
{
	public static class StringUtils
	{
		public static string ToUrlSafeBase64(this byte[] input)
		{
			return Convert.ToBase64String(input).Replace("+", "-").Replace("/", "_");
		}

		public static byte[] FromUrlSafeBase64(this string input)
		{
			return Convert.FromBase64String(input.Replace("-", "+").Replace("_", "/"));
		}

		public static string ToBase64Url(this string input)
		{
			var readOnlyInput = input.AsMemory();
			var writebleInput = Unsafe.As<ReadOnlyMemory<char>, Memory<char>>(ref readOnlyInput);
			foreach (ref var elem in writebleInput.Span)
			{
				if (elem == '-')
					elem = '+';
				else if (elem == '_')
					elem = '/';
			}
			return input;
		}
	}
}
