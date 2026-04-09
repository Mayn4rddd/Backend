using backend.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using backend.DTOs;
using backend.Models;
namespace backend.Controllers;



[ApiController]
[Route("api/student")]
public class StudentController : ControllerBase
{
    private readonly AppDbContext _context;

    public StudentController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("section/{sectionId}")]
    public IActionResult GetStudents(int sectionId, int sessionId)
    {
        var students = _context.Students
            .Where(s => s.SectionId == sectionId)
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.StudentId,

                Status = _context.Attendance
                    .Where(a => a.StudentId == s.Id && a.AttendanceSessionId == sessionId)
                    .Select(a => a.Status)
                    .FirstOrDefault() ?? AttendanceStatus.NotMarked
            })
            .ToList();

        return Ok(students);
    }

    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var student = _context.Students
            .FirstOrDefault(s => s.StudentId == dto.StudentId && s.Name == dto.Name);

        if (student == null)
            return NotFound("Student not found");

        var user = _context.Users.FirstOrDefault(u => u.Id == student.UserId);
        if (user == null)
            return NotFound("User not found");

        user.Password = dto.NewPassword;

        _context.SaveChanges();

        return Ok(new { message = "Password reset successful" });
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterStudentDto dto)
    {
        var student = _context.Students
            .FirstOrDefault(s => s.StudentId == dto.StudentId && s.Name == dto.Name);

        if (student == null || student.IsRegistered)
            return BadRequest("Invalid student");

        var user = new User
        {
            Name = dto.Name,
            Password = dto.Password,
            Role = "Student"
        };

        _context.Users.Add(user);

        student.UserId = user.Id;
        student.IsRegistered = true;

        _context.SaveChanges();

        return Ok(new { message = "Registered successfully" });
    }
}