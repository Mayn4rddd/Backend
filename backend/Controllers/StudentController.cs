using backend.Enums;
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
            .ToList();

        var attendance = _context.Attendance
            .Where(a => a.AttendanceSessionId == sessionId)
            .ToList();

        var result = students.Select(s =>
        {
            var record = attendance
                .FirstOrDefault(a => a.StudentDbId == s.Id);

            return new
            {
                studentId = s.StudentId,
                studentName = s.Name,
                status = record?.Status ?? AttendanceStatus.NotMarked
            };
        });

        return Ok(result);
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

        user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword); ;

        _context.SaveChanges();

        return Ok(new { message = "Password reset successful" });
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterStudentDto dto)
    {
        var student = _context.Students
            .FirstOrDefault(s => s.StudentId == dto.StudentId);

        if (student == null)
            return BadRequest("Student ID not found");

        if (student.IsRegistered)
            return BadRequest("Student already registered");

        var usernameExists = _context.Users.Any(u => u.Username == dto.Username);
        if (usernameExists)
            return BadRequest("Username already taken");

        var user = new User
        {
            Name = student.Name,
            Username = dto.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
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