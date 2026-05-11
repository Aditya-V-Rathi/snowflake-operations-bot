using Microsoft.AspNetCore.Mvc;
using SnowflakeBot.API.Services;

namespace SnowflakeBot.API.Controllers
{
    [ApiController]
    [Route("api/audit")]
    public class AuditController : ControllerBase
    {
        private readonly AuditService _auditService;

        public AuditController(AuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet("logs")]
        public IActionResult GetLogs()
        {
            var logs = _auditService.GetAll();
            return Ok(logs);
        }

        [HttpGet("users")]
        public IActionResult GetManagedUsers()
        {
            var users = _auditService.GetManagedUsers();
            return Ok(users);
        }
    }
}