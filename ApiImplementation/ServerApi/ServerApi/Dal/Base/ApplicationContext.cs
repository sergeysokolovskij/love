using Microsoft.EntityFrameworkCore;

namespace ServerApi.Dal
{
	public class ApplicationContext : DbContext
	{

		public ApplicationContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<AuthStorage> AuthStorages { get; set; }
		public DbSet<Session> Sessions { get; set; }
		public DbSet<Message> Messages { get; set; }
		public DbSet<StrongKey> StrongKeys { get; set; }
	}
}
