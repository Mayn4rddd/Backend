using Microsoft.AspNetCore.Mvc;
using System.Linq;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // LOGIN
    [HttpPost("login")]
    public IActionResult Login(string name, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Name == name);
        if (user == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            return Unauthorized(new { message = "Invalid password setup" });

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return Unauthorized();

        return Ok(new
        {
            message = "Login success",
            role = user.Role
        });
    }

    // 🔥 METHOD 1: GENERATE HASH (FOR SSMS FIXING)
    [HttpGet("generate-hash")]
    public IActionResult GenerateHash(string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        return Ok(new { hash });
    }
}