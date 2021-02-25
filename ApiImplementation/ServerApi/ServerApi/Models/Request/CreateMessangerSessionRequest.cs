using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerApi.Models.Request
{
	public class CreateMessangerSessionRequest
	{
		[JsonProperty("PublicKey")]
		public string PublicKey { get; set; } // публичный ключ для клиента
	}
}
