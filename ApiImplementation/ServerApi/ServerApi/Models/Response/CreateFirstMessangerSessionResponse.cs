using System;
using System.Collections.Generic;
using System.Text;

namespace ServerApi.Models.Response
{
	public class CreateFirstMessangerSessionResponse
	{
		public string ServerPublicKey { get; set; }
		public string CryptedAes { get; set; }
	}
	public class CreateMessangerSessionResponse
	{
		public string ServerPublicKey { get; set; }
		public string SessionId { get; set; }
	}
}
