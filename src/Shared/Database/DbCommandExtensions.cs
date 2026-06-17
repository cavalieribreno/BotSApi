using System.Data.Common;

namespace BotSaaS.Api.Shared.Database;

// Reusable extensions in DbCommand
public static class DbCommandExtensions
{
    public static void AddParameter(this DbCommand command, string name, object value)
    {
        DbParameter p = command.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        command.Parameters.Add(p);
    }
} 