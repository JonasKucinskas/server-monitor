using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/command")]
public class CommandController : ControllerBase
{
    public CommandController()
    {
    }

    [HttpPost("execute")]
    public IActionResult ExecuteFunction([FromBody] CommandRequest request)
    {
        if (string.IsNullOrEmpty(request?.Command))
        {
            return BadRequest(new { message = "Command string is required" });
        }

        string cmdOutput = MultiSshConnection.Instance.RunCmd("localhost", 12345, request.Command);
        Console.WriteLine($"Function executed with command: {request.Command}");
        return Ok(new { output = cmdOutput });
    }
}

public class CommandRequest
{
    public string Command { get; set; }
}