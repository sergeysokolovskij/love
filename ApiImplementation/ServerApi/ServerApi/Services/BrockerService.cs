using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServerApi.Dal;
using ServerApi.RealTime;
using ServerApi.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerApi.Services
{
	public interface IBrockerService
	{
		Task PuplishReadMessage(string message);
	}

	public class BrockerService : IBrockerService
	{
		private readonly ApplicationContext db;
		private readonly IBrockerConnectionProvider connectionsProvider;
		private readonly ILogger logger;


		public BrockerService(IBrockerConnectionProvider connectionsProvider,
			ApplicationContext db,
			ILoggerFactory loggerFactory)
		{
			this.connectionsProvider = connectionsProvider;
			this.logger = loggerFactory.CreateLogger<BrockerService>();
			this.db = db;
		}

		public async Task PuplishReadMessage(string message)
        {
			using (var connection = connectionsProvider.GetConnection().CreateModel())
            {
				var session = await db.Sessions.SingleOrDefaultAsync();
				string routingkey = BrockerKeysFactory.GenerateQueueKey(session.SessionId, BrcokerKeysTypes.readmessage);
			
				connection.BasicPublish("readedmessages", routingkey, null, Encoding.UTF8.GetBytes(message));
            }
        }
	}
}
