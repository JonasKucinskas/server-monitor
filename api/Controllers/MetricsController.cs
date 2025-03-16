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
    public async Task<ActionResult<List<NetworkMetric>>> GetMetrics([FromQuery] string systemName)
    {
        try
        {
            DateTime now = DateTime.Now;
            DateTime dayago = now.AddMinutes(-1);

            var metrics = await _dbService.FetchServerMetrics(systemName, dayago, now);

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
}


