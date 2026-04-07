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

    [HttpPost("reset-password")]
    public IActionResult ResetPassword(
        [FromForm] string studentId,
        [FromForm] string name,
        [FromForm] string newPassword)
    {
        var student = _context.Students
            .FirstOrDefault(s => s.StudentId == studentId && s.Name == name);

        if (student == null)
            return NotFound("Student not found");

        var user = _context.Users.FirstOrDefault(u => u.Id == student.UserId);
        if (user == null)
            return NotFound("User not found");

        // ✅ NO HASHING
        user.Password = newPassword;

        _context.SaveChanges();

        return Ok(new { message = "Password reset successful" });
    }

    [HttpPost("register")]
    public IActionResult Register(
        [FromForm] string studentId,
        [FromForm] string name,
        [FromForm] string password)
    {
        var student = _context.Students
            .FirstOrDefault(s => s.StudentId == studentId && s.Name == name);

        if (student == null || student.IsRegistered)
            return BadRequest("Invalid student");

        var user = new User
        {
            Name = name,
            Password = password,   // ✅ NO HASH
            Role = "Student"
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        student.UserId = user.Id;
        student.IsRegistered = true;

        _context.SaveChanges();

        return Ok(new { message = "Registered successfully" });
    }
}