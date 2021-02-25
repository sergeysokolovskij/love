using Api.DAL;
using Api.DAL.Base;
using Api.Services.Auth;
using Api.Services.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Processing
{
    public static class ConfigurationExtensions
    {
        public static void ConfigurAuthorization(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Lockout.MaxFailedAccessAttempts = 20;
                //options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationContext>()
            .AddDefaultTokenProviders();
        }

        public static void ConfigureSecurity(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
            });
            serviceCollection.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                options.HttpsPort = 443; // стандартный SSL-порт (Apache/nginx)
            });
        }

        public static void ConfigurAuthentication(this IServiceCollection serviceCollection, IConfiguration configuration, bool isDevelopment)
        {
            var audiences = configuration["Auth:Audience"].Split(',');

            var signingKey = new SignInSymmetricKey(configuration["TokenOptions:Key"]);
            var decryptionKey = new JwtCrypt(configuration["TokenOptions:CypherKey"]);

            var signingDecodingKey = (IJwtSigningDecodingKey)signingKey;
            var decryptKey = (IJwtEncryptingDecodingKey)decryptionKey;
            serviceCollection
                           .AddAuthentication(options =>
                           {
                               options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                               options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                               options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                           })
                           .AddJwtBearer(cfg =>
                           {
                               cfg.RequireHttpsMetadata = false;
                               cfg.TokenValidationParameters = new TokenValidationParameters
                               {
                                   ValidIssuer = configuration["Auth:Issuer"],
                                   ValidAudiences = audiences,
                                   IssuerSigningKey = signingDecodingKey.GetKey(),
                                   TokenDecryptionKey = decryptKey.GetKey(),
                                   ClockSkew = TimeSpan.Zero,
                                   ValidateLifetime = true,
                                   ValidateAudience = false,
                                   ValidateIssuer = true,
                                   ValidateIssuerSigningKey = true
                               };

                               cfg.Events = new JwtBearerEvents
                               {
                                   OnMessageReceived = context =>
                                   {
                                       var accessToken = context.Request.Query["access_token"];
                                       var path = context.HttpContext.Request.Path;

                                       if (!string.IsNullOrEmpty(accessToken))
                                           context.Token = accessToken;
                                       return Task.CompletedTask;
                                   }
                               };
                           });
        }

        public static void CorsConfiguration(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var cors = configuration.GetValue<string>(ConfigurationConstants.CorsNameInConfig).Split(",");

            serviceCollection.AddCors(options => options.AddPolicy(ConfigurationConstants.NameCorsPolicy,
                policyOptions =>
                {
                    policyOptions.WithOrigins(cors)
                                 .AllowAnyMethod()
                                 .AllowAnyHeader()
                                 .AllowCredentials()
                                 .WithExposedHeaders();
                }));
        }

        public static void ConfigureDbContext(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddDbContext<ApplicationContext>(options => options.UseMySql(configuration.GetConnectionString("MySql"))
                , ServiceLifetime.Transient);

        }

        public static void ConfigEncoders(this IServiceCollection serviceCollection)
        {
        }

        public static void CorsConfiguration(this IApplicationBuilder appBuilder)
        {
            appBuilder.UseCors(ConfigurationConstants.NameCorsPolicy);
        }

        public static void DbMigrationsEnable(this IApplicationBuilder appBuilder)
        {
            using (var scope = appBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>())
                {
                    try
                    {
                        if (db.Database.GetPendingMigrations().Any())
                            db.Database.Migrate();
                    }
                    catch { }
                }
            }
        }

        public static void SetCommonConfig(this IApplicationBuilder builder)
        {
            using (var scope = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                bool isDevelopment = config["ASPNETCORE_ENVIRONMENT"] == "Development";
                if (isDevelopment)
                    ErrorsMode.SetErrorsMode(true);
                else
                    ErrorsMode.SetErrorsMode(false);
            }
        }

        public static void AddRabbitMq(this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
        }
    }

    public class Configuration
    {
        public static bool IsConfigOkey = false;
        public static bool IsConnectToDbSuccess = false;
        public static string ErrorMessage = string.Empty;


        public static void InitialConfig()
        {
            bool isInstalled = true;

            string pathToConfigs = PathConstants.ConfigFolderName;

            if (!Directory.Exists(pathToConfigs))
            {
                Directory.CreateDirectory(pathToConfigs);
                isInstalled = false;
            }

            if (!Directory.Exists(PathConstants.ErrorsFolderName))
            {
                Directory.CreateDirectory(PathConstants.ErrorsFolderName);
                File.Create(Path.Combine(PathConstants.ErrorsFolderName, "logs.txt"));
                File.Create(Path.Combine(PathConstants.ErrorsFolderName, "errors.txt"));
            }
            if (!Directory.Exists(PathConstants.TempFileFolderName))
                Directory.CreateDirectory(PathConstants.TempFileFolderName);
            if (!Directory.Exists(PathConstants.PhotoFolderName))
                Directory.CreateDirectory(PathConstants.PhotoFolderName);

            string buildPath(string fileName)
            {
                return Path.Combine(PathConstants.ConfigFolderName, fileName);
            }

            string GetJsonString(object data)
            {
                return JsonConvert.SerializeObject(data);
            }

            string contentToWrite = string.Empty;

            string pathToFile = buildPath(PathConstants.authConfigName);

            if (!File.Exists(pathToFile))
            {
                File.AppendAllText(pathToFile, GetJsonString(new
                {
                    CrsfKey = ConfigurationConstants.StandartCrsfKey,
                    Auth = new
                    {
                        Issuer = ConfigurationConstants.StandartIssuer,
                        Audience = ConfigurationConstants.StandartAudience
                    }
                }));
                isInstalled = false;
            }

            pathToFile = buildPath(PathConstants.corsConfigName);

            if (!File.Exists(pathToFile))
            {
                File.AppendAllText(pathToFile, GetJsonString(new
                {
                    Cors = ConfigurationConstants.StandartCors
                }));
                isInstalled = false;
            }

            pathToFile = buildPath(PathConstants.globalConfig);
            if (!File.Exists(pathToFile))
            {
                ErrorMessage = InstallErrorMessage.GlobalConfigFileNotExist;

                File.AppendAllText(pathToFile, GetJsonString(new
                {
                    Api = ConfigurationConstants.StandartApiUrl,
                    Site = ConfigurationConstants.StandartApiSiteUrl
                }));
                isInstalled = false;
            }

            if (!isInstalled)
                return;

            IsConfigOkey = true;
        }

        public static void CheckAccessToDb(ApplicationContext db)
        {
            if (db.Database.CanConnect())
                IsConnectToDbSuccess = true;

            ErrorMessage = InstallErrorMessage.CannotConnectToDb;
        }
    }

    public class InstallErrorMessage
    {
        public const string CannotConnectToDb = "Невозможно присоеденится к базе данных. Нет прав или пароль указан неверно.";
        public const string DbConfigFileNotExist = "Файл конфигурации базы данных был создан. Заполните файл конфигурации базы данных.";
        public const string CorsFileNotExist = "Файл конфигурации CORS был создан и заполнен стандартными значениями. Рекомендуется дополнительно сконфигурировать CORS-файл";
        public const string GlobalConfigFileNotExist = "Глобальный файл конфигурации был создан и заполнен начальными значениями. Отредактируйте их и запустите сервер";
    }

    public class ConfigurationConstants
    {
        public const string NameCorsPolicy = "ShopPlatformPolicy";
        public const string CorsNameInConfig = "Cors";

        //Открыты стандартные соеденения. 3000 порт - разработка, галп , 8080 - стандартные NGinx/Apache порты, конечно, на продакшне лучше оставить последний
        public const string StandartCors = "http://localhost:3000,https://localhost:3000,http://localhost:8080,https://localhost:8080,https://localhost:44390,http://localhost:5000";
        public const string StandartAudience = "http://localhost:3000,https://localhost:3000,http://localhost:8080,https://localhost:8080,http://localhost:3000,http://localhost:5000";

        public const string StandartCrsfKey = "secret"; //ключ для шифрования crsf-токена
        public const string StandartIssuer = "ShopPlatformApi";

        public const string StandartApiUrl = "https://localhost:5001/";
        public const string StandartApiSiteUrl = "http://localhost:3000";
    }

    public class PathConstants
    {
        public const string ConfigFolderName = "Configs";
        public const string PhotoFolderName = "Pictures";
        public const string TempFileFolderName = "TempFiles";
        public const string ErrorsFolderName = "Errors";

        public const string authConfigName = "authConfig.json";
        public const string corsConfigName = "corsconfig.json";
        public const string dbConfig = "dbconfig";
        public const string globalConfig = "globalConfig.json";
    }

    public class HeaderNameConstants
    {

    }
}
