using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/systems")]
public class SystemsController : ControllerBase
{
    private readonly Database _dbService;

    public SystemsController(Database dbService)
    {
        _dbService = dbService;
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<SystemData>>> GetAllSystems([FromQuery] int userId)
    {
        try
        {
            var systems = await _dbService.FetchAllSystems(userId);

            var json = System.Text.Json.JsonSerializer.Serialize(systems);
            if (systems == null || systems.Count == 0)
            {
                return NotFound("No systems found.");
            }
            return Ok(systems);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("byname")]
    public async Task<ActionResult<SystemData>> GetSystemByName([FromQuery] string name)
    {
        try
        {
            var system = await _dbService.FetchSystemByName(name);

            var json = System.Text.Json.JsonSerializer.Serialize(system);
            if (system == null)
            {
                return NotFound("No systems found.");
            }
            return Ok(system);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}


