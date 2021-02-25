using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerApi;
using ServerApi.ApiCall;
using ServerApi.Dal;
using ServerApi.Models.Response;
using ServerApi.Options;
using ServerApi.RealTime;
using ServerApi.Services;
using ServerApi.Utils;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ChatClient1
{
	class Program
	{
		private static IHost BuildHost() =>
			Host.CreateDefaultBuilder()
			.ConfigureAppConfiguration((context, configure) =>
			{
				configure.AddJsonFile($"appsettings.Development.json");
			})
			.ConfigureServices((context, services) =>
			{
				services.AddOptions();
				services.AddHttpClient();

				services.AddDbContext<ApplicationContext>(options =>
					options.UseMySql(context.Configuration.GetConnectionString("MySql")), ServiceLifetime.Transient);

				services.Configure<UrlOptions>(context.Configuration.GetSection("EndPoints"));

				services.AddScoped<IAuthRequest, AuthRequest>();
				services.AddScoped<IMessangerSessionRequest, MessangerSessionRequest>();
				services.AddScoped<IConnectionProvider, ConnectionProvider>();
				services.AddScoped<IRsaCryptService, RsaCryptService>();
				services.AddScoped<IAesCipher, AesCryptService>();
				services.AddScoped<ICreateSessionService, CreateSessionService>();
				services.AddScoped<IMessageHubService, MessageHubService>();
				services.AddScoped<IMessangerService, MessangerService>();
				services.AddScoped<IUserStatusService, UserStatusService>();

				services.AddSingleton<IBrockerService, BrockerService>();
				services.AddSingleton<IBrockerConnectionProvider, BrockerConnectionProvider>();

			})
			.ConfigureLogging((context, logger)=> 
			{
				logger.SetMinimumLevel(LogLevel.Warning);
				logger.AddConsole();
			}).Build();
		public static async Task Main(string[] args)
		{
			var host = BuildHost();

			var authRequest = host.Services.GetRequiredService<IAuthRequest>();
			var brockerService = host.Services.GetRequiredService<IBrockerService>();
			await brockerService.PuplishReadMessage("test");
			await authRequest.GetAuthTokenAsync();
			var messangerSessionRequest = host.Services.GetRequiredService<IMessangerSessionRequest>();
			var receiveMessageService = host.Services.GetRequiredService<IMessageHubService>();
			var db = host.Services.GetRequiredService<ApplicationContext>();

			var userStatusService = host.Services.GetRequiredService<IUserStatusService>();

			var messangerService = host.Services.GetRequiredService<IMessangerService>();
			await messangerSessionRequest.SetServerSessionAsync();

			var connProvider = host.Services.GetRequiredService<IConnectionProvider>();
			var currentSession = await db.Sessions.SingleOrDefaultAsync();

			var connection = await connProvider.MakeHubConnectionAsync();
			//brockerService.SubscribeToQuee(currentSession.SessionId);

			connection.On("ReceiveMesssage", async (string message) => await receiveMessageService.ReceiveMessageAsync(message));
			connection.On("UserOnlineNotify", (string userId) => userStatusService.OnlineUser(userId));
			connection.On("UserOfflineNotify", (string userId) => userStatusService.OfflineUser(userId));

			await connection.StartAsync();

			var cancelerationTokenSource = new CancellationTokenSource();
			var cancelToken = cancelerationTokenSource.Token;

			Console.WriteLine("Введите сообщение:");
			string messageToSend = Console.ReadLine();
			while (messageToSend != "stop")
			{
				string cryptedMessage = await messangerService.GetMessageToSendAsync(messageToSend);
				await connection.SendAsync("SendMessage", cryptedMessage, cancelToken);

				Console.WriteLine("Введите следующее сообщение:");
				messageToSend = Console.ReadLine();
			}

			await host.RunAsync();
		}
	}
}
