using System;
using System.Collections.Generic;
using System.Text;

namespace ServerApi.Dal
{
	public class AuthStorage
	{
		public long Id { get; set; }
		public string AuthToken { get; set; }
		public string RefreshToken { get; set; }
	}
}
