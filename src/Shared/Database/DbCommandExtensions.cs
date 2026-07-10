using System.Data.Common;

namespace BotSaaS.Api.Shared.Database;

public record Parameter(string Name, object Value);

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
    public static void AddParameters(this DbCommand command, List<Parameter> parameters)
    {
        foreach(var parameter in parameters)
        {
            DbParameter p = command.CreateParameter();
            p.ParameterName = parameter.Name;
            p.Value = parameter.Value;
            command.Parameters.Add(p);
        }
    }
} 