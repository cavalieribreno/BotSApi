using System.Data.Common;
using MySqlConnector;

namespace BotSaaS.Api.Shared.Database;

// Class MySqlConnection Driver
public class MySqlDatabase : IDatabase
{
    private readonly string _connectionString;
    public MySqlDatabase(){
        string dbHost = Environment.GetEnvironmentVariable("DB_HOST")
            ?? throw new InvalidOperationException("DB_HOST não definido");
        string dbUser = Environment.GetEnvironmentVariable("DB_USER")
            ?? throw new InvalidOperationException("DB_USER não definido");
        string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
            ?? throw new InvalidOperationException("DB_PASSWORD não definido");
        string dbName = Environment.GetEnvironmentVariable("DB_NAME")
            ?? throw new InvalidOperationException("DB_NAME não definido");
        string dbPort = Environment.GetEnvironmentVariable("DB_PORT")
            ?? throw new InvalidOperationException("DB_PORT não definido");

        string connectionString = $"Server={dbHost};User={dbUser};Password={dbPassword};Database={dbName};Port={dbPort}";

        _connectionString = connectionString;                                             
    }
    
    public DbConnection CreateConnection()
    {
        MySqlConnection connection = new MySqlConnection(_connectionString);
        return connection;
    }
}