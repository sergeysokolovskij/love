using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServerApi.ApiCall;
using ServerApi.Dal;
using ServerApi.Models.Request;
using ServerApi.Models.Response;
using ServerApi.Options;
using ServerApi.Services;
using ServerApi.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ServerApi
{
	public interface IAuthRequest
	{
		Task<T> MakeHttpRequestAsync<T>(string url, HttpMethod httpMethod, object body = null, int countUnauthorizedRequest = 0);
		Task<T> MakeFormHttpRequestAsync<T>(string url, Dictionary<string, string> values, int countUnauthorizedRequest = 0);
		Task<string> MakeHttpRequestWithTextResultAsync(string url, HttpMethod httpMethod, object body, int countUnauthorizedRequest = 0);
		Task MakeHttpReqeustWithoutResponseAsync(string url, HttpMethod httpMethod, object body = null, int countUnauthorizedRequest = 0);
		Task<AuthResult> GetAuthTokenAsync(bool isNeedNewAuth = false);
		Task CreateSessionAsync();
		Task<DynamicConfig> GetDynamicConfigAsync();
	}
	public class AuthRequest : BaseApiCall, IAuthRequest
	{
		private readonly ApplicationContext Context;
		private readonly IConfiguration Configuration;

		private readonly IRsaCryptService rsaCryptService;
		private readonly IAesCipher aes;

		public AuthRequest(
			IHttpClientFactory httpClientFactory,
			IOptions<UrlOptions> urlOptions,
			ApplicationContext context,
			IRsaCryptService rsaCryptoService,
			IConfiguration configuration,
			IAesCipher aes):base(httpClientFactory, urlOptions)
		{
			Context = context;
			this.rsaCryptService = rsaCryptoService;
			this.aes = aes;
			this.Configuration = configuration;
		}

		private async Task<HttpResponseMessage> MakeSimpleHttpRequestAsync(string url, HttpMethod method, object body)
		{
			var httpRequestMessage = new HttpRequestMessage();
			httpRequestMessage.Method = method;
			httpRequestMessage.RequestUri = new Uri(UrlOptions.Value.BaseUrl + url);

			if (body != null)
			{
				var json = JsonConvert.SerializeObject(body);
				var jsonBody = new StringContent(json, Encoding.UTF8, "application/json");
				httpRequestMessage.Content = jsonBody;
			}

			var result = await httpClient.SendAsync(httpRequestMessage);

			return result;
		}

		private async Task<HttpResponseMessage> MakeSimpleFormHttpRequestAsync(string url, Dictionary<string, string> values)
		{
			var httpRequestMessage = new HttpRequestMessage();
			httpRequestMessage.Method = HttpMethod.Post;

			var form = new MultipartFormDataContent();

			foreach (KeyValuePair<string, string> value in values)
				form.Add(new StringContent(value.Value), value.Key);

			httpRequestMessage.RequestUri = new Uri(httpClient.BaseAddress + url);
			httpRequestMessage.Content = form;

			var response = await httpClient.SendAsync(httpRequestMessage);
			return response;
		}

		public async Task<T> MakeHttpRequestAsync<T>(string url, HttpMethod httpMethod, object body = null, int countUnauthorizedRequest = 0)
		{
			var httpResponseMessage = await MakeSimpleHttpRequestAsync(url, httpMethod, body);
			await HttpErrorHandle(httpResponseMessage);

			if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
			{
				if (countUnauthorizedRequest < 3)
				{
					var authResult = await GetAuthTokenAsync();
					return await MakeHttpRequestAsync<T>(url, httpMethod, body, ++countUnauthorizedRequest);
				}
				throw new Exception("User is unauthorized");
			}
			if (httpResponseMessage.StatusCode == HttpStatusCode.Forbidden)
			{
				if (countUnauthorizedRequest < 3)
				{
					var authResult = await GetAuthTokenAsync(true);
					return await MakeHttpRequestAsync<T>(url, httpMethod, body, ++countUnauthorizedRequest);
				}
			}

			var response = await httpResponseMessage.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<T>(response);
		}

		public async Task<T> MakeFormHttpRequestAsync<T>(string url, Dictionary<string, string> values, int countUnauthorizedRequest = 0)
		{
			var httpResponseMessage = await MakeSimpleFormHttpRequestAsync(url, values);
			await HttpErrorHandle(httpResponseMessage);

			if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
			{
				if (countUnauthorizedRequest < 3)
				{
					var token = await GetAuthTokenAsync();

					return await MakeFormHttpRequestAsync<T>(url, values);
				}

				throw new Exception("User is anautorized");
			}

			var stream = await httpResponseMessage.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<T>(stream);

			return result;
		}

		public async Task<string> MakeHttpRequestWithTextResultAsync(string url, HttpMethod httpMethod, object body, int countUnauthorizedRequest = 0)
		{
			var httpResponseMessage = await MakeSimpleHttpRequestAsync(url, httpMethod, body);
			await HttpErrorHandle(httpResponseMessage);

			if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
			{
				if (countUnauthorizedRequest < 3)
				{
					var authResult = await GetAuthTokenAsync();

					return await MakeHttpRequestAsync<string>(url, httpMethod, body, ++countUnauthorizedRequest);
				}
				throw new Exception("User is unauthorized");
			}

			var result = await httpResponseMessage.Content.ReadAsStringAsync();
			return result;
		}

		public async Task MakeHttpReqeustWithoutResponseAsync(string url, HttpMethod httpMethod, object body = null, int countUnauthorizedRequest = 0)
		{
			var httpResponseMessage = await MakeSimpleHttpRequestAsync(url, httpMethod, body);
			await HttpErrorHandle(httpResponseMessage);

			if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
			{
				if (countUnauthorizedRequest < 3)
				{
					var authResult = await GetAuthTokenAsync();
					await MakeHttpReqeustWithoutResponseAsync(url, httpMethod, body);
				}
				throw new Exception("Unauthorized");
			}
		}

		private async Task<AuthResult> GetAuthResultAsync()
		{
			var userAccounts = await MakeHttpRequestAsync<IList<UserAccount>>(UrlOptions.Value.DevAccountsUrl, HttpMethod.Get);
			var currentUser = userAccounts[Configuration.GetValue<int>("AccId")];

			var authResponse = await MakeFormHttpRequestAsync<AuthResult>(UrlOptions.Value.AuthUrl, new Dictionary<string, string>()
			{
					{"userName", currentUser.Login },
					{"password", currentUser.Password }
			});

			return authResponse;
		}

		private async Task RenewAuthResult(AuthStorage storage, AuthResult result)
		{
			storage.AuthToken = result.AccessToken;
			storage.RefreshToken = result.RefreshToken;

			Context.Entry(storage).State = EntityState.Modified;
			await Context.SaveChangesAsync();
		}

		public async Task<DynamicConfig> GetDynamicConfigAsync()
		{
			var response = await MakeHttpRequestAsync<DynamicConfig>(UrlOptions.Value.DynamicConfigUrl, HttpMethod.Get);
			return response;
		}

		public async Task<AuthResult> GetAuthTokenAsync(bool isNeedNewAuth = false)
		{
			var result = await MakeAuthTokenAsync(isNeedNewAuth);
			return result;
		}


		private async Task<AuthResult> MakeAuthTokenAsync(bool isNeedNewAuth = false)
		{
			var authStorage = await Context.AuthStorages.SingleOrDefaultAsync();
			if (authStorage == null) 
			{
				var authResponse = await GetAuthResultAsync();
				Context.AuthStorages.Add(new AuthStorage()
				{
					AuthToken = authResponse.AccessToken,
					RefreshToken = authResponse.RefreshToken
				});

				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
				await Context.SaveChangesAsync();

				return authResponse;
			}

			if (authStorage.AuthToken == null || authStorage.RefreshToken == null)
			{
				var authResponse = await GetAuthResultAsync();

				authStorage.AuthToken = authResponse.AccessToken;
				authStorage.RefreshToken = authResponse.RefreshToken;

				Context.Entry(authStorage).State = EntityState.Modified;
				await Context.SaveChangesAsync();

				return authResponse;
			}

			if (isNeedNewAuth)
			{
				string requesObj = await new 
				{
					refreshToken = authStorage.RefreshToken
				}.ToJson();

				var updateRequest = BuildRequestMessage(UrlOptions.Value.UpdateRefreshTokenUrl, HttpMethod.Post, requesObj);
				var updateTokenResult = await httpClient.SendAsync(updateRequest);

				string updateTokenContent = await updateTokenResult.Content.ReadAsStringAsync();
				var authResponse = JsonConvert.DeserializeObject<AuthResult>(updateTokenContent);

				authStorage.AuthToken = authResponse.AccessToken;
				authStorage.RefreshToken = authResponse.RefreshToken;

				Context.Entry(authStorage).State = EntityState.Modified;

				await Context.SaveChangesAsync();

				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);

				return authResponse;
			}

			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authStorage.AuthToken);
			var authResponseMessage = await httpClient.GetAsync(UrlOptions.Value.BaseUrl + UrlOptions.Value.AuthInfoUrl);
			
			if (authResponseMessage.StatusCode == HttpStatusCode.OK)
			{
				var authStream = await authResponseMessage.Content.ReadAsStringAsync();
				var authInfo = JsonConvert.DeserializeObject<AuthUserInfo>(authStream);

				return new AuthResult()
				{
					AccessToken = authStorage.AuthToken,
					RefreshToken = authStorage.RefreshToken,
					Roles = authInfo.Roles,
					UserName = authInfo.Login
				};
			}

			var jsonObj = new
			{
				refreshToken = authStorage.RefreshToken
			};

			string json = await jsonObj.ToJson();
			StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

			var httpRequest = new HttpRequestMessage();

			httpRequest.Method = HttpMethod.Post;
			httpRequest.Content = content;
			httpRequest.RequestUri = new Uri(UrlOptions.Value.BaseUrl + UrlOptions.Value.UpdateRefreshTokenUrl);

			var httpResult = await httpClient.SendAsync(httpRequest);

			//в случае просрочки refresh
			if (httpResult.StatusCode == HttpStatusCode.Unauthorized)
			{
				var authResult = await GetAuthResultAsync();
				await RenewAuthResult(authStorage, authResult);
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

				return authResult;
			}

			var stream = await httpResult.Content.ReadAsStringAsync();
			var authObj = JsonConvert.DeserializeObject<AuthResult>(stream);

			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authObj.AccessToken);

			await RenewAuthResult(authStorage, authObj);

			return authObj;
		}

		public async Task CreateSessionAsync()
		{
			var rsaPair = rsaCryptService.GenerateKeys();
			var strongKeys = await Context.StrongKeys.SingleOrDefaultAsync();

			if (strongKeys == null)
			{
				var firstSessionRequestModel = new CreateMessangerSessionRequest()
				{
					PublicKey = rsaPair.publicKey
				};

				string jsonRequest = await firstSessionRequestModel.ToJson();

				var firstSessionResponse = await GetStringFromHttpResultAsync(UrlOptions.Value.CreateFirstSessionUrl, HttpMethod.Post, jsonRequest);
				var response = JsonConvert.DeserializeObject<CreateFirstMessangerSessionResponse>(firstSessionResponse);
				
				byte[] decryptedAesKey = rsaCryptService.Decrypt(rsaPair.privateKey, response.CryptedAes).FromUrlSafeBase64(); //на этом этапе имеем расшифрованый ключ aes для работы

				Context.StrongKeys.Add(new StrongKey()
				{
					Cypher = decryptedAesKey
				});

				await Context.SaveChangesAsync();

				await GetAuthTokenAsync(true);

				rsaPair = rsaCryptService.GenerateKeys();
				string cryptedPublicKey = await aes.Crypt(rsaPair.publicKey);

				var sessionRequestModel = new CreateMessangerSessionRequest()
				{
					PublicKey = cryptedPublicKey
				};

				jsonRequest = await sessionRequestModel.ToJson();
				var sessionRequest = BuildRequestMessage(UrlOptions.Value.CreateSessionUrl, HttpMethod.Post, jsonRequest);

				var sessionResponseMessage = await httpClient.SendAsync(sessionRequest);
				sessionResponseMessage.EnsureSuccessStatusCode();

				var sessionResponse = JsonConvert.DeserializeObject<CreateMessangerSessionResponse>(await sessionResponseMessage.Content.ReadAsStringAsync());

				string decryptedServerPublicKey = await aes.Decrypt(sessionResponse.ServerPublicKey);
				string decryptedSessionId = await aes.Decrypt(sessionResponse.SessionId);

				Context.Sessions.Add(new Session()
				{
					ClientPublicKey = rsaPair.publicKey,
					ClientPrivateKey = rsaPair.privateKey,
					ServerPublicKey = decryptedServerPublicKey,
					Created = DateTime.Now,
					SessionId = decryptedSessionId
				});
			}
			else
			{
				await GetAuthTokenAsync(true);

				string cryptedPublicKey = await aes.Crypt(rsaPair.publicKey);

				var createSessionModel = new CreateMessangerSessionRequest()
				{
					PublicKey = cryptedPublicKey
				};

				string jsonRequest = await createSessionModel.ToJson();

				var responseMessage = await GetStringFromHttpResultAsync(UrlOptions.Value.CreateSessionUrl, HttpMethod.Post, jsonRequest);
				var response = JsonConvert.DeserializeObject<CreateMessangerSessionResponse>(responseMessage);

				var session = await Context.Sessions.SingleOrDefaultAsync();
				if (session == null)
					session = new Session();
				string decryptedServerPublicKey = await aes.Decrypt(response.ServerPublicKey);

				session.ServerPublicKey = decryptedServerPublicKey;
				session.ClientPrivateKey = rsaPair.privateKey;
				session.ClientPublicKey = rsaPair.publicKey;

				Context.Entry(session).State = EntityState.Modified;
			}
			await Context.SaveChangesAsync();
		}


		private async Task HttpErrorHandle(HttpResponseMessage httpResponseMessage)
		{
			if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
				throw new Exception($"Url adress not found: {httpResponseMessage.RequestMessage.RequestUri}");
			if (httpResponseMessage.StatusCode == HttpStatusCode.InternalServerError)
			{
				var responseMessage = await httpResponseMessage.Content.ReadAsStringAsync();
				throw new Exception($"Internal erver error.\nMessage: {responseMessage}");
			}
		}
	}
}
