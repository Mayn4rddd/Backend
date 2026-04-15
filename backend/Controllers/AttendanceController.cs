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
        var student = _context.Students
            .FirstOrDefault(s => s.Id == dto.StudentId);

        if (student == null)
            return BadRequest("Student not found");

        var today = DateTime.UtcNow.Date;

        var session = _context.AttendanceSessions
            .FirstOrDefault(s =>
                s.SectionId == dto.SectionId &&
                s.SubjectId == dto.SubjectId &&
                s.TeacherId == dto.TeacherId &&
                s.Mode == "Manual" &&
                s.StartTime.Date == today
            );

        if (session == null)
        {
            session = new AttendanceSession
            {
                SectionId = dto.SectionId,
                SubjectId = dto.SubjectId,
                TeacherId = dto.TeacherId,
                StartTime = DateTime.UtcNow,
                Mode = "Manual"
            };

            _context.AttendanceSessions.Add(session);
            _context.SaveChanges();
        }

        var alreadyMarked = _context.Attendance.Any(a =>
            a.StudentDbId == student.Id &&
            a.AttendanceSessionId == session.Id
        );

        if (alreadyMarked)
            return BadRequest("Student already marked for today");

        var attendance = new Attendance
        {
            StudentDbId = student.Id,
            SectionId = dto.SectionId,
            SubjectId = dto.SubjectId,
            TeacherId = dto.TeacherId,
            Timestamp = DateTime.UtcNow,
            Status = dto.Status,
            AttendanceSessionId = session.Id
        };

        _context.Attendance.Add(attendance);
        _context.SaveChanges();

        return Ok(new
        {
            message = "Manual attendance recorded",
            studentName = student.Name,
            status = dto.Status,
            date = attendance.Timestamp.ToString("yyyy-MM-dd")
        });
    }

    [HttpPost("scan")]
    public IActionResult Scan([FromBody] ScanDto dto)
    {
        var token = dto.Token;

        var now = DateTime.UtcNow;

        var session = _context.QrSessions
            .FirstOrDefault(s => s.Token == token);

        if (session == null)
            return BadRequest("Invalid session");

        if (session.Expiry < now)
            return BadRequest("Session expired");

        var student = _context.Students
            .FirstOrDefault(s => s.StudentId == dto.StudentId);

        if (student == null)
            return BadRequest("Student not found");

        var exists = _context.Attendance.Any(a =>
            a.StudentDbId == student.Id &&
            a.AttendanceSessionId == session.AttendanceSessionId
        );

        if (exists)
            return BadRequest("Already scanned for this session");

        var lateThreshold = session.StartTime.AddMinutes(10);

        string status = now > lateThreshold
            ? AttendanceStatus.Late
            : AttendanceStatus.Present;

        var attendance = new Attendance
        {
            StudentDbId = student.Id,
            SectionId = session.SectionId,
            SubjectId = session.SubjectId,
            TeacherId = session.TeacherId,
            Timestamp = now,
            Status = status,
            QrSessionId = session.Id,
            AttendanceSessionId = session.AttendanceSessionId
        };

        _context.Attendance.Add(attendance);
        _context.SaveChanges();

        return Ok(new
        {
            message = "Attendance recorded",
            studentName = student.Name,
            status
        });
    }

    [HttpGet("live/{sessionId}")]
    public IActionResult GetLiveAttendance(int sessionId)
    {
        var data = _context.Attendance
            .Where(a => a.AttendanceSessionId == sessionId)
            .Select(a => new
            {
                studentName = _context.Students
                    .Where(s => s.Id == a.StudentDbId)
                    .Select(s => s.Name)
                    .FirstOrDefault(),

                studentId = _context.Students
                    .Where(s => s.Id == a.StudentDbId)
                    .Select(s => s.StudentId)
                    .FirstOrDefault(),

                section = _context.Sections
                    .Where(sec => sec.Id == a.SectionId)
                    .Select(sec => sec.Name)
                    .FirstOrDefault(),

                status = a.Status,
                time = a.Timestamp
            })
            .OrderByDescending(x => x.time)
            .ToList();

        return Ok(data);
    }
}