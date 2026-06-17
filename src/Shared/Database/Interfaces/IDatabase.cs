using System.Data.Common;

namespace BotSaaS.Api.Shared.Database;

// Interface to database connection
public interface IDatabase
{
    public DbConnection CreateConnection();
}