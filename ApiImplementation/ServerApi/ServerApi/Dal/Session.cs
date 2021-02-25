using System;
using System.Collections.Generic;
using System.Text;

namespace ServerApi.Dal
{
	public class Session
	{
		public long Id { get; set; }
		public string SessionId { get; set; }
		public string ClientPublicKey { get; set; }
		public string ClientPrivateKey { get; set; }
		public string ServerPublicKey { get; set; }
		public DateTime Created { get; set; }
	}
}
