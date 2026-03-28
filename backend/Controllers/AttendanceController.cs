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
    public IActionResult Scan(Guid sessionId, string token, int studentId)
    {
        var session = _context.QrSessions
            .FirstOrDefault(s => s.SessionId == sessionId);

        if (session == null || session.ExpiryTime < DateTime.Now)
            return BadRequest("Session expired");

        if (session.Token != token)
            return BadRequest("Invalid token");

        var exists = _context.Attendance.Any(a =>
            a.StudentId == studentId && a.ClassId == session.ClassId);

        if (exists)
            return BadRequest("Already scanned");

        var attendance = new Attendance
        {
            StudentId = studentId,
            ClassId = session.ClassId,
            Timestamp = DateTime.Now,
            Status = "Present"
        };

        _context.Attendance.Add(attendance);
        _context.SaveChanges();

        return Ok("Attendance recorded");
    }
}