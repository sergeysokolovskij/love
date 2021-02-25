using Microsoft.Extensions.Options;
using ServerApi.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ServerApi.ApiCall
{
	public class BaseApiCall
	{
		protected readonly IOptions<UrlOptions> UrlOptions;
		protected HttpClient httpClient;

		public BaseApiCall(
			IHttpClientFactory httpClientFactory,
			IOptions<UrlOptions> urlOptions)
		{
			this.UrlOptions = urlOptions;

			httpClient = httpClientFactory.CreateClient();
			httpClient.BaseAddress = new Uri(urlOptions.Value.BaseUrl);
			httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");

		}

		protected HttpRequestMessage BuildRequestMessage(string uri, HttpMethod method, string content = null)
		{
			var httpRequestMessage = new HttpRequestMessage();

			httpRequestMessage.RequestUri = new Uri(UrlOptions.Value.BaseUrl + uri);
			httpRequestMessage.Method = method;

			if (!string.IsNullOrEmpty(content))
			{
				StringContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
				httpRequestMessage.Content = httpContent;
			}

			return httpRequestMessage;
		}


		protected async Task<string> GetStringFromHttpResultAsync(string uri, HttpMethod method, string content = null)
		{
			var request = BuildRequestMessage(uri, method, content);
			var response = await httpClient.SendAsync(request);

			response.EnsureSuccessStatusCode();

			var result = await response.Content.ReadAsStringAsync();

			return result;
		}
	}
}
