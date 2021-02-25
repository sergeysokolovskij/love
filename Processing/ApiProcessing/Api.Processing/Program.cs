using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Api.DAL.Base;
using Api.Services.Brocker;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.Processing
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			IHostBuilder hostBuilder = CreateHostBuilder(args);

			Configuration.InitialConfig();

			IHost host = null;

			host = hostBuilder.Build();

			var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

			try
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

				if (Configuration.IsConfigOkey)
				{
					Configuration.CheckAccessToDb(dbContext);
					if (Configuration.IsConnectToDbSuccess)
					{
						try
						{
							dbContext.Migrate();
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
					}
					else
						Console.WriteLine("Невозможно подключится к бд");
				}
				else
				{
					Console.WriteLine(Configuration.ErrorMessage);
				}
				dbContext.Dispose();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}


			var brockerServcie = scope.ServiceProvider.GetRequiredService<IBrockerService>();
			brockerServcie.SubscribeToQuee("testquee");

			for (int i = 0; i < 5; i++)
			{
				await Task.Delay(2000);
				brockerServcie.PublishMessage("testquee", "dsadasdsadas");
			}

			await host.RunAsync();
		}
		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();

					webBuilder.ConfigureAppConfiguration((builderContext, config) =>
					{
						string env = builderContext.HostingEnvironment.EnvironmentName;
						string dbSettingsFile = $"{PathConstants.ConfigFolderName}/{PathConstants.dbConfig}.{env}.json";
						string authSettingsFile = Path.Combine(PathConstants.ConfigFolderName, PathConstants.authConfigName);
						string corsConfigFile = Path.Combine(PathConstants.ConfigFolderName, PathConstants.corsConfigName);
						string globalConfigFile = Path.Combine(PathConstants.ConfigFolderName, PathConstants.globalConfig);

						config.AddJsonFile(dbSettingsFile);
						config.AddJsonFile(authSettingsFile);
						config.AddJsonFile(corsConfigFile);
						config.AddJsonFile(globalConfigFile);
					});
				})
				.ConfigureLogging((logger) =>
				{
					logger.AddConsole();
					logger.SetMinimumLevel(LogLevel.Information);
					logger.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
					logger.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
				});
		}
	}
}
