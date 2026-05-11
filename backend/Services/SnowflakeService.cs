using Snowflake.Data.Client;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace SnowflakeBot.API.Services
{
    public class SnowflakeService
    {
        private readonly string _connectionString;

        public SnowflakeService(IConfiguration configuration)
        {
            var sf = configuration.GetSection("Snowflake");
            _connectionString = $"account={sf["Account"]};user={sf["User"]};password={sf["Password"]};warehouse={sf["Warehouse"]};role=ACCOUNTADMIN;insecureMode=false";
        }

        /// <summary>
        /// Generates a secure random password that meets Snowflake requirements.
        /// </summary>
        private string GenerateSecurePassword()
        {
            const int length = 16;
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*";
            
            var allChars = uppercase + lowercase + digits + specialChars;
            var password = new StringBuilder();
            
            using (var rng = new RNGCryptoServiceProvider())
            {
                var randomData = new byte[length];
                rng.GetBytes(randomData);
                
                // Ensure password includes at least one of each character type
                password.Append(uppercase[randomData[0] % uppercase.Length]);
                password.Append(lowercase[randomData[1] % lowercase.Length]);
                password.Append(digits[randomData[2] % digits.Length]);
                password.Append(specialChars[randomData[3] % specialChars.Length]);
                
                // Fill remaining characters randomly
                for (int i = 4; i < length; i++)
                {
                    password.Append(allChars[randomData[i] % allChars.Length]);
                }
            }
            
            return password.ToString();
        }

        public string OnboardUser(string username, string role)
        {
            try
            {
                using var conn = new SnowflakeDbConnection();
                conn.ConnectionString = _connectionString;
                conn.Open();

                // Explicitly set database and schema
                var useCmd = conn.CreateCommand();
                useCmd.CommandText = "USE DATABASE SNOWFLAKE_DB;";
                useCmd.ExecuteNonQuery();

                var useSchema = conn.CreateCommand();
                useSchema.CommandText = "USE SCHEMA PUBLIC;";
                useSchema.ExecuteNonQuery();

                // Generate secure temporary password
                var tempPassword = GenerateSecurePassword();

                // Create user
                var createCmd = conn.CreateCommand();
                createCmd.CommandText = $"CREATE USER IF NOT EXISTS {username} PASSWORD='{tempPassword}' MUST_CHANGE_PASSWORD=TRUE;";
                createCmd.ExecuteNonQuery();

                // Assign role
                var roleCmd = conn.CreateCommand();
                roleCmd.CommandText = $"GRANT ROLE {role} TO USER {username};";
                roleCmd.ExecuteNonQuery();

                return $"User '{username}' created and assigned role '{role}' successfully.";
            }
            catch (Exception ex)
            {
                throw new Exception($"Snowflake error: {ex.Message}");
            }
        }

        public string ResetPassword(string username)
        {
            try
            {
                using var conn = new SnowflakeDbConnection();
                conn.ConnectionString = _connectionString;
                conn.Open();

                var useCmd = conn.CreateCommand();
                // Generate secure temporary password
                var tempPassword = GenerateSecurePassword()
                useCmd.ExecuteNonQuery();

                var tempPassword = $"Reset@{new Random().Next(10000, 99999)}!Temp";

                var cmd = conn.CreateCommand();
                cmd.CommandText = $"ALTER USER {username} SET PASSWORD='{tempPassword}' MUST_CHANGE_PASSWORD=TRUE;";
                cmd.ExecuteNonQuery();

                return $"Password reset for '{username}'. Temporary password: {tempPassword}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Snowflake error: {ex.Message}");
            }
        }
    }
}