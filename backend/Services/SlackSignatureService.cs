using System.Security.Cryptography;
using System.Text;

namespace SnowflakeBot.API.Services
{
    public class SlackSignatureService
    {
        private readonly string _signingSecret;

        public SlackSignatureService(IConfiguration configuration)
        {
            _signingSecret = configuration["Slack:SigningSecret"];
        }

        public bool IsValidRequest(string timestamp, string signature, string rawBody)
        {
            // Prevent replay attacks - reject requests older than 5 minutes
            var requestTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp));
            if (Math.Abs((DateTimeOffset.UtcNow - requestTime).TotalMinutes) > 5)
            {
                Console.WriteLine("[WARN] Slack request rejected: timestamp too old");
                return false;
            }

            // Build the base string exactly as Slack does
            var baseString = $"v0:{timestamp}:{rawBody}";

            // HMAC-SHA256 hash using signing secret
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_signingSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));
            var computedSignature = "v0=" + BitConverter.ToString(hash).Replace("-", "").ToLower();

            // Compare computed signature with Slack's signature
            var isValid = computedSignature == signature;
            if (!isValid)
            {
                Console.WriteLine("[WARN] Slack request rejected: invalid signature");
            }
            return isValid;
        }
    }
}