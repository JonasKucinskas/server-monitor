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
        if (string.IsNullOrEmpty(request.Command))
        {
            return BadRequest(new { message = "Command string is required" });
        }

        if (request.SystemInfo == null || string.IsNullOrEmpty(request.SystemInfo.ip))
        {
            return BadRequest(new { message = "System information is required" });
        }

        string cmdOutput = SshConnection.Instance.RunCmd(request.SystemInfo.ip, request.SystemInfo.port, request.Command);
        Console.WriteLine($"Function executed with command: {request.Command}. output: {cmdOutput}");
        return Ok(new { output = cmdOutput });
    }
}
public class CommandRequest
{
    public string Command { get; set; }
    public SystemData SystemInfo { get; set; }
}