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
    public async Task<ActionResult<List<NetworkMetric>>> GetMetrics()
    {
        try
        {
            DateTime now = DateTime.Now;
            DateTime dayago = now.AddHours(-24);

            var metrics = await _dbService.FetchServerMetrics("localhost", dayago, now);
            var json = System.Text.Json.JsonSerializer.Serialize(metrics);
            if (metrics == null || !metrics.Any())
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


