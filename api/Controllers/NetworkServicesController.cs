using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations; 

[ApiController]
[Route("api/networkServices")]
public class NetworkServicesController : ControllerBase
{
    private readonly Database _dbService;

    public NetworkServicesController(Database dbService)
    {
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SystemData>>> GetAllServices([FromQuery] string systemId)
    {
        try
        {
            var services = await _dbService.FetchAllNetworkServices(systemId);

            var json = System.Text.Json.JsonSerializer.Serialize(services);
            if (services == null)
            {
                return NotFound("No systems found.");
            }
            return Ok(services);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<SystemData>> AddService([FromBody] NetworkService newService)
    {
        try
        {
            if (newService == null)
            {
                return BadRequest("Service data is null.");
            }

            await _dbService.InsertNetworkService(newService);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("pings")]
    public async Task<ActionResult<List<SystemData>>> GetPings([FromQuery, Required] int serviceId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            DateTime now = DateTime.Now;
            DateTime dayago = now.AddMinutes(-5);
            dayago = dayago.AddHours(-2);

            DateTime actualStartDate = startDate ?? dayago;
            DateTime actualEndDate = endDate ?? now;

            Console.WriteLine(actualStartDate);
            Console.WriteLine(actualEndDate);

            var pings = await _dbService.FetchNetworkServicePings(serviceId, actualStartDate, actualEndDate);

            var json = System.Text.Json.JsonSerializer.Serialize(pings);
            return Ok(pings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}


