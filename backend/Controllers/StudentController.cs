using Microsoft.AspNetCore.Mvc;
using System.Linq;

[ApiController]
[Route("api/student")]
public class StudentController : ControllerBase
{
    private readonly AppDbContext _context;

    public StudentController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public IActionResult Register(string studentId, string name, string password)
    {
        var student = _context.Students
            .FirstOrDefault(s => s.StudentId == studentId && s.Name == name);

        if (student == null || student.IsRegistered)
            return BadRequest("Invalid student");

        var user = new User
        {
            Name = name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Student"
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        student.UserId = user.Id;
        student.IsRegistered = true;

        _context.SaveChanges();

        return Ok("Registered successfully");
    }
}