using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models.Options;
using Api.Providers;
using Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Api.Processing
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.OptionsConfigure(Configuration);
			services.ConfigureDbContext(Configuration);
			services.RegisterProviders();
			services.RegisterServices(Configuration);
			services.AddRabbitMq(Configuration);

			bool isDevelopment = Configuration["ASPNETCORE_ENVIRONMENT"] == "Development";

			services.ConfigurAuthorization();
			services.ConfigurAuthentication(Configuration, isDevelopment);


			services.AddHttpContextAccessor();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGet("/", async context =>
				{
					await context.Response.WriteAsync("Processing service is work");
				});
			});
		}
	}
}
