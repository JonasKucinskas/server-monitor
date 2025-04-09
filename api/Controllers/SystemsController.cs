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
            var systems = await _dbService.FetchAllSystemsByUserId(userId);

            var json = System.Text.Json.JsonSerializer.Serialize(systems);
            if (systems == null || systems.Count == 0)
            {
                return NotFound("No systems found.");
            }
            return Ok(systems);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"server error: {ex.Message}");
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
            return StatusCode(500, $"server error: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<SystemData>> CreateSystem([FromBody] SystemData newSystem)
    {
        try
        {
            if (newSystem == null)
            {
                return BadRequest("System data is required.");
            }

            var existingSystem = await _dbService.FetchSystemByName(newSystem.name);
            Console.WriteLine(existingSystem.name);
            if (existingSystem.name != null)
            {
                return Conflict("A system already exist.");
            }

            await _dbService.InsertSystemAsync(newSystem);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"server error: {ex.Message}");
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteSystem([FromQuery]int id)
    {
        try
        {
            await _dbService.DeleteSystemAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"server error: {ex.Message}");
        }
    }

    [HttpPut]
    public async Task<ActionResult<SystemData>> UpdateSystem([FromBody] SystemData newService)
    {
        try
        {
            if (newService == null)
            {
                return BadRequest("Service data is null.");
            }

            await _dbService.UpdateSystem(newService);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
