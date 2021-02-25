using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ServerApi.Models.Response
{
	public class AuthUserInfo
	{
		[JsonProperty("login")]
		public string Login { get; set; }
		[JsonProperty("roles")]
		public List<string> Roles { get; set; }
		[JsonProperty("isPhoneConfirmed")]
		public bool IsPhoneConfirmed { get; set; }
		[JsonProperty("isEmailConfirmed")]
		public bool IsEmailConfirmed { get; set; }
	}
}
