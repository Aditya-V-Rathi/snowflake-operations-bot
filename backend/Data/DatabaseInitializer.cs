using Microsoft.Data.Sqlite;

namespace SnowflakeBot.API.Data
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SQLite");
        }

        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS AuditLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SlackUserId TEXT NOT NULL,
                    SlackUsername TEXT NOT NULL,
                    Command TEXT NOT NULL,
                    Parameters TEXT,
                    Status TEXT NOT NULL,
                    Message TEXT,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS ManagedUsers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Role TEXT NOT NULL,
                    CreatedBy TEXT NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );
            ";
            command.ExecuteNonQuery();
        }
    }
}