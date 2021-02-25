using System;
using System.Collections.Generic;
using System.Text;

namespace ServerApi.Options
{
	public class UrlOptions
	{
		public string BaseUrl { get; set; }
		public string AuthUrl { get; set; }
		public string AuthInfoUrl { get; set; }
		public string DevAccountsUrl { get; set; }
		public string AuthUserInfoUrl { get; set; }
		public string UpdateRefreshTokenUrl { get; set; }
		public string CreateFirstSessionUrl { get; set; }
		public string CreateSessionUrl { get; set; }
		public string DynamicConfigUrl { get; set; }
		public string SetServerSessionUrl { get; set; }
	}
}
