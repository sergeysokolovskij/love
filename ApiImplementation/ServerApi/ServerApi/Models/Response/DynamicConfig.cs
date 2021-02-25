using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerApi.Models.Response
{
	public class DynamicConfig
	{
		[JsonProperty("tokenOptions")]
		public TokenOptionsModel TokenOptions  { get; set; }
		public class TokenOptionsModel
		{
			[JsonProperty("acessLifeTime")]
			public int AcessTokenLifeTime { get; set; }
			[JsonProperty("refresh")]
			public int RefreshTokenLifeTime { get; set; }
		}
	}
}
