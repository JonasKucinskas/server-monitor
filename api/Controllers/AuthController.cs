using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims; 
using System.Text; 
using Microsoft.IdentityModel.Tokens; 

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{

    private readonly Database _dbService;

    public AuthController(Database dbService)
    {
        _dbService = dbService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<JwtSecurityToken>> Login([FromBody] AuthRequest request)
    {
        var user = await _dbService.GetUser(request.Username, request.Password);

        if (user == null)
        {
            return Unauthorized("Invalid credentials");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, "user")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey12345_superSecretKey12345"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "monitor",
            audience: "monitor",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new 
        { 
            token = jwt,
            user = new 
            {
                user.id, 
                user.username, 
                user.creationDate
            }
        });

    }

    [HttpPost("register")]
    public async Task<ActionResult<JwtSecurityToken>> Register([FromBody] User user)
    {
        try
        {
            bool userExists = await _dbService.DoesUserExist(user.username);
            if (userExists)
            {
                return Ok("User already exists");;
            }
            await _dbService.InsertUser(user);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

public class AuthRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}