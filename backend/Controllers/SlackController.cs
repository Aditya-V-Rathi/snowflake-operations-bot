using Microsoft.AspNetCore.Mvc;
using SnowflakeBot.API.Services;
using System.IO;

namespace SnowflakeBot.API.Controllers
{
    [ApiController]
    [Route("api/slack")]
    public class SlackController : ControllerBase
    {
        private readonly SlackService _slackService;
        private readonly SlackSignatureService _slackSignatureService;

        public SlackController(SlackService slackService, SlackSignatureService slackSignatureService)
        {
            _slackService = slackService;
            _slackSignatureService = slackSignatureService;
        }

        [HttpPost("command")]
        public async Task<IActionResult> HandleCommand()
        {
            // Read raw body for signature verification
            Request.EnableBuffering();
            var rawBody = await new StreamReader(Request.Body).ReadToEndAsync();
            Request.Body.Position = 0;

            // Get Slack headers
            var timestamp = Request.Headers["X-Slack-Request-Timestamp"].ToString();
            var signature = Request.Headers["X-Slack-Signature"].ToString();

            // Verify the request is genuinely from Slack
            if (!_slackSignatureService.IsValidRequest(timestamp, signature, rawBody))
            {
                return Unauthorized("Invalid Slack signature");
            }

            // Parse form fields manually since we already read the body
            var form = await Request.ReadFormAsync();
            var userId = form["user_id"].ToString();
            var userName = form["user_name"].ToString();
            var text = form["text"].ToString();
            var responseUrl = form["response_url"].ToString();

            // Fire and forget — respond to Slack immediately within 3 seconds
            _ = Task.Run(() => _slackService.HandleCommand(userId, userName, text, responseUrl));

            return Ok(new { text = "Processing your request..." });
        }
    }
}