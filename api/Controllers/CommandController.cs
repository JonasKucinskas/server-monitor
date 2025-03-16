using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/command")]
public class CommandController : ControllerBase
{
    public CommandController()
    {
    }

    [HttpPost("execute")]
    public IActionResult ExecuteFunction([FromQuery] string Command, [FromBody] SystemData systemInfo)
    {
        if (string.IsNullOrEmpty(Command))
        {
            return BadRequest(new { message = "Command string is required" });
        }

        if (systemInfo == null || string.IsNullOrEmpty(systemInfo.ip))
        {
            return BadRequest(new { message = "System information is required" });
        }

        string cmdOutput = MultiSshConnection.Instance.RunCmd(systemInfo.ip, systemInfo.port, Command);
        Console.WriteLine($"Function executed with command: {Command}");
        return Ok(new { output = cmdOutput });
    }
}