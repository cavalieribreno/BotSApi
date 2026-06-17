using System.Data.Common;

namespace BotSaaS.Api.Shared.Database;

// Holds the connection + transaction of the current request.
// Scoped: every repo in the request shares the SAME instance.
public class DbSession
{
    public DbConnection Connection { get; set; } = null!;
    public DbTransaction Transaction { get; set; } = null!;
}