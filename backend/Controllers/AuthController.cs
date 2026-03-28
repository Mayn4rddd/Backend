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

    [HttpPost("login")]
    public IActionResult Login(string name, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Name == name);
        if (user == null) return Unauthorized();

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return Unauthorized();

        return Ok(new { message = "Login success", user.Role });
    }
}