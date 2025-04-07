using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/key")]
public class KeyController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<string>> GetKey()
    {
        try
        {
            var key = await System.IO.File.ReadAllTextAsync("publicKey.pub");

            if (string.IsNullOrWhiteSpace(key))
            {
                return NotFound("No key found in the file.");
            }
            return Ok(key);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}