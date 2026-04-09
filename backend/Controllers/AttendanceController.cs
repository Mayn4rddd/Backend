using backend.DTOs;
using backend.Enums;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;


namespace backend.Controllers;


[ApiController]
[Route("api/attendance")]
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _context;

    public AttendanceController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("manual")]
    public IActionResult ManualAttendance([FromBody] ManualAttendanceDto dto)
    {
        var exists = _context.Attendance.Any(a =>
            a.StudentId == dto.StudentId &&
            a.AttendanceSessionId == dto.SessionId
        );

        if (exists)
            return BadRequest("Already marked");

        var attendance = new Attendance
        {
            StudentId = dto.StudentId,
            SectionId = dto.SectionId,
            SubjectId = dto.SubjectId,
            TeacherId = dto.TeacherId,
            Timestamp = DateTime.UtcNow,
            Status = dto.Status,
            AttendanceSessionId = dto.SessionId
        };

        _context.Attendance.Add(attendance);
        _context.SaveChanges();

        return Ok(new { message = "Manual attendance recorded" });
    }

    [HttpPost("close-session")]
    public IActionResult CloseSession(int sessionId)
    {
        var session = _context.AttendanceSessions
            .FirstOrDefault(s => s.Id == sessionId);

        if (session == null)
            return NotFound();

        var students = _context.Students
            .Where(s => s.SectionId == session.SectionId)
            .ToList();

        foreach (var student in students)
        {
            var exists = _context.Attendance.Any(a =>
                a.StudentId == student.Id &&
                a.AttendanceSessionId == sessionId
            );

            if (!exists)
            {
                _context.Attendance.Add(new Attendance
                {
                    StudentId = student.Id,
                    SectionId = session.SectionId,
                    SubjectId = session.SubjectId,
                    TeacherId = session.TeacherId,
                    Timestamp = DateTime.UtcNow,
                    Status = "Absent",
                    AttendanceSessionId = sessionId
                });
            }
        }

        _context.SaveChanges();

        return Ok("Session closed. Absentees marked.");
    }

    [HttpPost("scan")]
    public IActionResult Scan(
     [FromForm] string token,
     [FromForm] int studentId)
    {
        var now = DateTime.UtcNow; // ✅ FIXED

        // 🔥 1. FIND SESSION
        var session = _context.QrSessions
            .FirstOrDefault(s => s.Token == token);

        if (session == null)
            return BadRequest("Invalid session");

        // 🔥 2. CHECK EXPIRY
        if (session.Expiry < now)
            return BadRequest("Session expired");

        // 🔥 3. PREVENT DUPLICATE (STRONG FIX 🔥)
        var exists = _context.Attendance.Any(a =>
     a.StudentId == studentId &&
     a.AttendanceSessionId == session.AttendanceSessionId
        );

        if (exists)
            return BadRequest("Already scanned for this session");

        // 🔥 4. DETERMINE STATUS
        var lateThreshold = session.StartTime.AddMinutes(10);

        string status = now > lateThreshold
    ? AttendanceStatus.Late
    : AttendanceStatus.Present;

        // 🔥 5. SAVE
        var attendance = new Attendance
        {
            StudentId = studentId,
            SectionId = session.SectionId,
            SubjectId = session.SubjectId,
            TeacherId = session.TeacherId,
            Timestamp = now,
            Status = status,
            QrSessionId = session.Id,  // ✅ IMPORTANT
            AttendanceSessionId = session.AttendanceSessionId
        };

        _context.Attendance.Add(attendance);
        _context.SaveChanges();

        return Ok(new
        {
            message = "Attendance recorded",
            status
        });
    }
}