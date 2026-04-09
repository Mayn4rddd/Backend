using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using backend.Models;
using backend.DTOs;

namespace backend.Controllers;



[ApiController]
[Route("api/qr")]
public class QrController : ControllerBase
{
    private readonly AppDbContext _context;

    public QrController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("generate")]
    public IActionResult GenerateQR([FromBody] GenerateQrDto dto)
    {
        var teacher = _context.Teachers
            .FirstOrDefault(t => t.UserId == dto.UserId);

        if (teacher == null)
            return NotFound("Teacher not found");

        // ✅ Validate teacher assignment
        var isAssigned = _context.TeacherAssignments.Any(x =>
            x.TeacherId == teacher.Id &&
            x.SectionId == dto.SectionId &&
            x.SubjectId == dto.SubjectId
        );

        if (!isAssigned)
            return BadRequest("You are not assigned to this section and subject");

        // 🔥 NEW: FIND OR CREATE ATTENDANCE SESSION (QR MODE)
        var attendanceSession = _context.AttendanceSessions
            .FirstOrDefault(s =>
                s.SectionId == dto.SectionId &&
                s.SubjectId == dto.SubjectId &&
                s.TeacherId == teacher.Id &&
                s.Mode == "QR"
            );

        if (attendanceSession == null)
        {
            attendanceSession = new AttendanceSession
            {
                SectionId = dto.SectionId,
                SubjectId = dto.SubjectId,
                TeacherId = teacher.Id,
                StartTime = DateTime.UtcNow,
                Mode = "QR"
            };

            _context.AttendanceSessions.Add(attendanceSession);
            _context.SaveChanges();
        }

        // 🔥 Generate QR token
        var token = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;

        // 🔥 Create QR session (linked to AttendanceSession)
        var qrSession = new QrSession
        {
            SectionId = dto.SectionId,
            SubjectId = dto.SubjectId,
            TeacherId = teacher.Id,
            Token = token,
            StartTime = now,
            Expiry = now.AddMinutes(5),
            AttendanceSessionId = attendanceSession.Id // ✅ LINK
        };

        _context.QrSessions.Add(qrSession);
        _context.SaveChanges();

        return Ok(new
        {
            qrSessionId = qrSession.Id,
            attendanceSessionId = attendanceSession.Id, // 🔥 IMPORTANT for frontend/manual
            token = qrSession.Token,
            expiresAt = qrSession.Expiry
        });
    }
}