using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ServerApi.Dal.Base
{
	public class ContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
	{
		public ApplicationContext CreateDbContext(string[] args)
		{
			var hostBuilder = Host.CreateDefaultBuilder().ConfigureAppConfiguration((context, configure) =>
			{
				configure.AddJsonFile($"appsettings.Development.json");
			}).Build();

			using var scope = hostBuilder.Services.GetService<IServiceScopeFactory>().CreateScope();
			var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

			var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
			optionsBuilder.UseMySql(config.GetConnectionString("MySql"));
			return new ApplicationContext(optionsBuilder.Options);
		}
	}
}
