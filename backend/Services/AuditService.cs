using Microsoft.Data.Sqlite;
using SnowflakeBot.API.Models;

namespace SnowflakeBot.API.Services
{
    public class AuditService
    {
        private readonly string _connectionString;

        public AuditService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SQLite");
        }

        public void Log(AuditLog log)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO AuditLogs (SlackUserId, SlackUsername, Command, Parameters, Status, Message, Timestamp)
                VALUES ($slackUserId, $slackUsername, $command, $parameters, $status, $message, $timestamp);
            ";
            command.Parameters.AddWithValue("$slackUserId", log.SlackUserId);
            command.Parameters.AddWithValue("$slackUsername", log.SlackUsername);
            command.Parameters.AddWithValue("$command", log.Command);
            command.Parameters.AddWithValue("$parameters", log.Parameters ?? "");
            command.Parameters.AddWithValue("$status", log.Status);
            command.Parameters.AddWithValue("$message", log.Message ?? "");
            command.Parameters.AddWithValue("$timestamp", log.Timestamp);
            command.ExecuteNonQuery();
        }

        public List<AuditLog> GetAll()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM AuditLogs ORDER BY Timestamp DESC;";

            var logs = new List<AuditLog>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                logs.Add(new AuditLog
                {
                    Id = reader.GetInt32(0),
                    SlackUserId = reader.GetString(1),
                    SlackUsername = reader.GetString(2),
                    Command = reader.GetString(3),
                    Parameters = reader.GetString(4),
                    Status = reader.GetString(5),
                    Message = reader.GetString(6),
                    Timestamp = reader.GetDateTime(7)
                });
            }
            return logs;
        }

        public void AddManagedUser(string username, string role, string createdBy)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR IGNORE INTO ManagedUsers (Username, Role, CreatedBy, CreatedAt)
                VALUES ($username, $role, $createdBy, $createdAt);
            ";
            command.Parameters.AddWithValue("$username", username.ToUpper());
            command.Parameters.AddWithValue("$role", role);
            command.Parameters.AddWithValue("$createdBy", createdBy);
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow);
            command.ExecuteNonQuery();
        }

        public bool IsManagedUser(string username)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM ManagedUsers WHERE Username = $username;";
            command.Parameters.AddWithValue("$username", username.ToUpper());
            var count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }

        public List<ManagedUser> GetManagedUsers()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM ManagedUsers ORDER BY CreatedAt DESC;";

            var users = new List<ManagedUser>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new ManagedUser
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Role = reader.GetString(2),
                    CreatedBy = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4)
                });
            }
            return users;
        }
    }
}