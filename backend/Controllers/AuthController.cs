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

    // ✅ WEB LOGIN (React)
    [HttpPost("web-login")]
    public IActionResult WebLogin([FromBody] LoginRequest request)
    {
        if (request == null ||
            string.IsNullOrEmpty(request.Name) ||
            string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Name or password is empty");
        }

        var user = _context.Users.FirstOrDefault(u => u.Name == request.Name);

        if (user == null)
            return Unauthorized(new { message = "User not found" });

        if (user.Password != request.Password)
            return Unauthorized(new { message = "Invalid password" });

        // 🚫 BLOCK STUDENTS HERE
        if (user.Role == "Student")
        {
            return Unauthorized(new
            {
                message = "Students cannot log in on web. Use the mobile app."
            });
        }

        return Ok(new
        {
            token = "sample-token",
            role = user.Role,
            name = user.Name,
            userId = user.Id
        });
    }
    
   
    // ✅ MOBILE LOGIN (Flutter)
    [HttpPost("mobile-login")]
    public IActionResult MobileLogin([FromBody] LoginRequest request)
    {
        if (request == null ||
            string.IsNullOrEmpty(request.Name) ||
            string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Name or password is empty");
        }

        var user = _context.Users.FirstOrDefault(u => u.Name == request.Name);

        if (user == null)
            return Unauthorized(new { message = "User not found" });

        if (user.Password != request.Password)
            return Unauthorized(new { message = "Invalid password" });

        // ✅ Students allowed here

        return Ok(new
        {
            token = "sample-token",
            role = user.Role,
            name = user.Name,
            userId = user.Id
        });
    }
}