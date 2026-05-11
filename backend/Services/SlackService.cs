using SnowflakeBot.API.Models;

namespace SnowflakeBot.API.Services
{
    public class SlackService
    {
        private readonly SnowflakeService _snowflakeService;
        private readonly AuditService _auditService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public SlackService(SnowflakeService snowflakeService, AuditService auditService, IConfiguration configuration, HttpClient httpClient)
        {
            _snowflakeService = snowflakeService;
            _auditService = auditService;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task HandleCommand(string userId, string username, string text, string responseUrl)
        {
            string message;
            string status = "success"; // default
            var operation = "unknown"; // default
            // Validate input
            if (string.IsNullOrWhiteSpace(text))
            {
                message = "Command text is empty. Usage: /snowflake <command> [args]";
                status = "failed";
            }
            else
            {
                var parts = text.Trim().Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                operation = parts[0].ToLower();

                try
                {
                    // Check if user is authorized
                    var authorizedUsers = _configuration.GetSection("AuthorizedSlackUsers").Get<List<string>>();
                    if (!authorizedUsers.Contains(userId))
                    {
                        message = "You are not authorized to perform Snowflake operations.";
                        status = "failed";
                    }
                    else
                    {
                        (message, status) = operation switch
                        {
                            "onboard_user" when parts.Length >= 3 
                                => (OnboardUser(parts[1], parts[2], username), "success"),
                            "onboard_user" 
                                => ("Usage: /snowflake onboard_user <username> <role>", "failed"),
                            "reset_password" when parts.Length >= 2 
                                => ResetPasswordWithStatus(parts[1]),
                            "reset_password" 
                                => ("Usage: /snowflake reset_password <username>", "failed"),
                            _ 
                                => ($"Unknown command '{operation}'. Supported: onboard_user, reset_password", "failed")
                        };
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    status = "failed";
                }
            }

            // Log to audit
            _auditService.Log(new AuditLog
            {
                SlackUserId = userId,
                SlackUsername = username,
                Command = operation,
                Parameters = text,
                Status = status,
                Message = message,
                Timestamp = DateTime.UtcNow
            });

            // Respond back to Slack
            try
            {
                var payload = new { text = message };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(responseUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Failed to send response to Slack. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception while sending response to Slack: {ex.Message}");
            }
        }
        private string OnboardUser(string username, string role, string createdBy)
        {
            var result = _snowflakeService.OnboardUser(username, role);
            // Only save to managed users if Snowflake succeeded
            _auditService.AddManagedUser(username, role, createdBy);
            return result;
        }

        private (string message, string status) ResetPasswordWithStatus(string username)
        {
            if (!_auditService.IsManagedUser(username))
            {
                return ($"User '{username}' is not managed by this system. Only onboarded users can have their password reset.", "failed");
            }
            return (_snowflakeService.ResetPassword(username), "success");
        }
    }
}