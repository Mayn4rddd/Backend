using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

[ApiController]
[Route("api/attendance")]
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _context;

    public AttendanceController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("scan")]
    public IActionResult Scan(
     [FromForm] Guid sessionId,
     [FromForm] string token,
     [FromForm] int studentId)
    {
        var session = _context.QrSessions
            .FirstOrDefault(s => s.SessionId == sessionId);

        if (session == null || session.ExpiryTime < DateTime.Now)
            return BadRequest("Session expired");

        if (session.Token != token)
            return BadRequest("Invalid token");

        var exists = _context.Attendance.Any(a =>
            a.StudentId == studentId && a.SectionId == session.SectionId);

        if (exists)
            return BadRequest("Already scanned");

        var attendance = new Attendance
        {
            StudentId = studentId,
            SectionId = session.SectionId,
            Timestamp = DateTime.Now,
            Status = "Present"
        };

        _context.Attendance.Add(attendance);
        _context.SaveChanges();

        return Ok(new { message = "Attendance recorded" });
    }
}

