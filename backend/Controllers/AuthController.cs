using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using backend.Models;
using backend.DTOs;

namespace backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("web-login")]
    public IActionResult WebLogin([FromBody] LoginRequest request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username or password is empty" });
        }

        var user = _context.Users
            .FirstOrDefault(u => u.Username == request.Username);

        if (user == null)
            return Unauthorized(new { message = "User not found" });

        if (user.Password != request.Password)
            return Unauthorized(new { message = "Invalid password" });

        if (user.Role == "Student")
        {
            return Unauthorized(new
            {
                message = "Students cannot log in on web. Use the mobile app."
            });
        }

        var token = Guid.NewGuid().ToString();

        return Ok(new
        {
            token = token,
            role = user.Role,
            username = user.Username,
            name = user.Name,
            userId = user.Id
        });
    }

    [HttpPost("mobile-login")]
    public IActionResult MobileLogin([FromBody] LoginRequest request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username or password is empty" });
        }

        var user = _context.Users
            .FirstOrDefault(u => u.Username == request.Username);

        if (user == null)
            return Unauthorized(new { message = "User not found" });

        if (user.Password != request.Password)
            return Unauthorized(new { message = "Invalid password" });

        var student = _context.Students
      .FirstOrDefault(s => s.UserId == user.Id);

        var token = Guid.NewGuid().ToString();
        return Ok(new
        {
            token = token,
            role = user.Role,
            username = user.Username,
            name = user.Name,
            userId = user.Id,
            studentId = student != null ? student.StudentId : null 
        });
    }
}