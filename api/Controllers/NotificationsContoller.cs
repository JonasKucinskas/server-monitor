using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly Database _dbService;

    public NotificationsController(Database dbService)
    {
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Notification>>> GetNotificationsByRuleId([FromQuery] int ruleId)
    {
        try
        {                   
            var notifications = await _dbService.FetchNotificationsByRuleIdAsync(ruleId, 0);//all found

            if (notifications == null)
            {
                return NotFound("No notifications found.");
            }
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet]
    [Route("latest")]
    public async Task<ActionResult<NetworkMetric>> GetLatestNotificationByRuleId([FromQuery] int ruleId)
    {
        try
        {
            var notifications = await _dbService.FetchNotificationsByRuleIdAsync(ruleId, 1);//single return

            if (notifications == null)
            {
                return NotFound("No metrics found.");
            }
            return Ok(notifications[0]);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete]
    public async Task<ActionResult<List<Notification>>> DeleteNotificationsById([FromQuery] int? ruleId, [FromQuery] int? notificationId)
    {
        try
        {
            if (ruleId.HasValue)
            {
                await _dbService.DeleteNotificationsByRuleIdAsync(ruleId.Value);
                return Ok();
            }
            else if (notificationId.HasValue)
            {
                await _dbService.DeleteNotificationByIdAsync(notificationId.Value);
                return Ok();
            }
            else
            {
                return BadRequest("Either ruleId or notificationId must be provided.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("rules")]
    public async Task<ActionResult<NotificationRule>> InsertNotificationRule([FromBody] NotificationRule rule)
    {
        try
        {
            if (rule == null)
            {
                return BadRequest("NotificationRule is required.");
            }

            var returnedRule = await _dbService.InsertNotificationRuleAsync(rule);
            return Ok(returnedRule);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"server error: {ex.Message}");
        }
    }

    [HttpDelete("rules")]
    public async Task<ActionResult<SystemData>> DeleteNotificationRule([FromQuery] int ruleId)
    {
        try
        {
            if (ruleId == null)
            {
                return BadRequest("NotificationRule id is required.");
            }
            await _dbService.DeleteNotificationRuleByIdAsync(ruleId);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"server error: {ex.Message}");
        }
    }

    [HttpGet("rules")]
    public async Task<ActionResult<List<NotificationRule>>> GetAllRules([FromQuery] string systemIp, [FromQuery] int userId)
    {
        if (string.IsNullOrEmpty(systemIp))
        {
            return BadRequest(new { message = "systemIp is required" });
        }

        try
        {
            var rules = await _dbService.FetchAllNotificationRulesAsync(systemIp, userId);

            if (rules == null)
            {
                return NotFound("No systems found.");
            }
            return Ok(rules);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("processes")]
    public async Task<ActionResult<List<Process>>> GetProcessesByNotificationId([FromQuery] int notificationId)
    {
        try
        {
            var processes = await _dbService.FetchProcessesByNotificationIdAsync(notificationId);

            if (processes == null)
            {
                return NotFound("No systems found.");
            }
            return Ok(processes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}