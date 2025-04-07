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

            await _dbService.InsertNetworkServiceAsync(newService);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut]
    public async Task<ActionResult<SystemData>> UpdateService([FromBody] NetworkService newService)
    {
        try
        {
            if (newService == null)
            {
                return BadRequest("Service data is null.");
            }

            await _dbService.UpdateNetworkService(newService);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete]
    public async Task<ActionResult<SystemData>> DeleteService([FromQuery] int serviceId)
    {
        try
        {
            if (serviceId == null)
            {
                return BadRequest("Service id is null.");
            }

            await _dbService.DeleteNetworkService(serviceId);
            await _dbService.DeleteNetworkServicePings(serviceId);
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

    [HttpGet("pings/latest")]
    public async Task<ActionResult<SystemData>> GetLatestPing([FromQuery, Required] int serviceId)
    {
        try
        {
            var latestPing = await _dbService.FetchLatestNetworkServicePing(serviceId);

            if (latestPing == null)
            {
                return NotFound($"No pings found for serviceId {serviceId}");
            }

            return Ok(latestPing);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}