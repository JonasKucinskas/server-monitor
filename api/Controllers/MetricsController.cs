using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/metrics")]
public class MetricsController : ControllerBase
{
    private readonly Database _dbService;

    public MetricsController(Database dbService)
    {
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NetworkMetric>>> GetMetrics([FromQuery] string systemName, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            DateTime now = DateTime.Now;
            DateTime dayago = now.AddMinutes(-11);

            DateTime actualStartDate = startDate ?? dayago;
            DateTime actualEndDate = endDate ?? now;

            var metrics = await _dbService.FetchServerMetrics(systemName, actualStartDate, actualEndDate);

            var json = System.Text.Json.JsonSerializer.Serialize(metrics);
            if (metrics == null || metrics.Count == 0)
            {
                return NotFound("No metrics found.");
            }
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet]
    [Route("latest")]
    public async Task<ActionResult<NetworkMetric>> GetLatestMetric([FromQuery] string systemName)
    {
        try
        {
            var metrics = await _dbService.FetchLatestMetrics(systemName);

            var json = System.Text.Json.JsonSerializer.Serialize(metrics);
            if (metrics == null)
            {
                return NotFound("No metrics found.");
            }
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}