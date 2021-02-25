using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ServerApi.Models.Response
{
	public class UserAccount
	{
		[JsonPropertyName("login")]
		public string Login { get; set; }
		[JsonPropertyName("password")]
		public string Password { get; set; }
	}
}
