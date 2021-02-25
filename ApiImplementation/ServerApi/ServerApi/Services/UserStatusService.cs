using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerApi.Services
{
    public interface IUserStatusService
    {
        void OnlineUser(string userId);
        void OfflineUser(string userId);
    }
    public class UserStatusService : IUserStatusService
    {

        private readonly ILogger<UserStatusService> logger;

        public UserStatusService(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<UserStatusService>();
        }

        public void OfflineUser(string userId)
        {
            Console.WriteLine($"User: {userId} is offline");
        }

        public void OnlineUser(string userId)
        {
            Console.WriteLine($"User: {userId} is online");
        }
    }
}
