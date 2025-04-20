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
            return BadRequest("Invalid credentials");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
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

    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("invalid token.");
            }

            if (string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest("password fields cannot be empty.");
            }

            var user = await _dbService.GetUserById(int.Parse(userId));
            if (user == null)
            {
                return NotFound("User not found.");
            }

            string old_hash = Hashing.HashPassword(request.OldPassword);
            
            if (old_hash != user.password)
            {
                return BadRequest("wrong old passowrd");
            }

            var newPasswordHash = Hashing.HashPassword(request.NewPassword);

            user.password = newPasswordHash;
            await _dbService.UpdateUserPasswordAsync(user);

            return Ok("Password successfully updated.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}

public class AuthRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}