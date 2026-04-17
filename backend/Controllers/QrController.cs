using backend.DTOs;
using backend.Helpers;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

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
        var now = TimeHelper.GetPhilippineTime();

        // 1. Get teacher
        var teacher = _context.Teachers
            .FirstOrDefault(t => t.UserId == dto.UserId);

        if (teacher == null)
            return NotFound("Teacher not found");

        // 2. Check assignment
        var assignment = _context.TeacherAssignments
            .FirstOrDefault(x =>
                x.TeacherId == teacher.Id &&
                x.SectionId == dto.SectionId &&
                x.SubjectId == dto.SubjectId
            );

        if (assignment == null)
            return BadRequest("You are not assigned to this section and subject");

        // 3. Get schedule for TODAY
        var todayDay = now.DayOfWeek.ToString();

        var schedule = _context.TeacherSchedules
            .FirstOrDefault(s =>
                s.TeacherAssignmentId == assignment.Id &&
                s.Day == todayDay
            );

        if (schedule == null)
            return BadRequest("No schedule for today");

        var startTime = TimeSpan.Parse(schedule.StartTime);
        var endTime = TimeSpan.Parse(schedule.EndTime);

        // 4. Check if within schedule
        if (now.TimeOfDay < startTime || now.TimeOfDay > endTime)
        {
            return BadRequest("You can only generate QR during your assigned time");
        }

        // 5. Create / get attendance session (per day)
        var attendanceSession = _context.AttendanceSessions
            .FirstOrDefault(s =>
                s.SectionId == dto.SectionId &&
                s.SubjectId == dto.SubjectId &&
                s.TeacherId == teacher.Id &&
                s.Mode == "QR" &&
                s.StartTime.Date == now.Date
            );

        if (attendanceSession == null)
        {
            attendanceSession = new AttendanceSession
            {
                SectionId = dto.SectionId,
                SubjectId = dto.SubjectId,
                TeacherId = teacher.Id,
                StartTime = now,
                Mode = "QR"
            };

            _context.AttendanceSessions.Add(attendanceSession);
            _context.SaveChanges();
        }

        // 🔥 6. CHECK IF QR ALREADY EXISTS (IMPORTANT)
        var existingQr = _context.QrSessions
            .Where(q => q.AttendanceSessionId == attendanceSession.Id)
            .OrderByDescending(q => q.StartTime)
            .FirstOrDefault();

        if (existingQr != null)
        {
            // ✅ RETURN OLD QR EVEN IF EXPIRED
            return Ok(new
            {
                attendanceSessionId = existingQr.AttendanceSessionId,
                qrSessionId = existingQr.Id,
                token = existingQr.Token,
                expiresAt = existingQr.Expiry,
                validUntil = existingQr.ScheduleEndTime
            });
        }

        // 7. Compute schedule end
        var scheduleEndDateTime = now.Date.Add(endTime);

        // 8. CREATE QR (ONLY ONCE)
        var qrSession = new QrSession
        {
            SectionId = dto.SectionId,
            SubjectId = dto.SubjectId,
            TeacherId = teacher.Id,
            Token = Guid.NewGuid().ToString(),
            StartTime = now,
            Expiry = now.AddMinutes(5), // Present window
            ScheduleEndTime = scheduleEndDateTime,
            AttendanceSessionId = attendanceSession.Id
        };

        _context.QrSessions.Add(qrSession);
        _context.SaveChanges();

        return Ok(new
        {
            attendanceSessionId = attendanceSession.Id,
            qrSessionId = qrSession.Id,
            token = qrSession.Token,
            expiresAt = qrSession.Expiry,
            validUntil = qrSession.ScheduleEndTime
        });
    }

    [HttpGet("active")]
public IActionResult GetActiveQr(int sectionId, int subjectId, int teacherId)
{
    var now = TimeHelper.GetPhilippineTime();

    // ✅ Convert UserId → Teacher.Id
    var teacher = _context.Teachers
        .FirstOrDefault(t => t.UserId == teacherId);

    if (teacher == null)
        return NotFound("Teacher not found");

    var qr = _context.QrSessions
        .Where(q =>
            q.SectionId == sectionId &&
            q.SubjectId == subjectId &&
            q.TeacherId == teacher.Id && // ✅ FIX HERE
            now <= q.ScheduleEndTime
        )
        .OrderByDescending(q => q.StartTime)
        .FirstOrDefault();

    if (qr == null)
        return NotFound("No active QR");

    return Ok(new
    {
        attendanceSessionId = qr.AttendanceSessionId,
        qrSessionId = qr.Id,
        token = qr.Token,
        expiresAt = qr.Expiry,
        validUntil = qr.ScheduleEndTime
    });
}
}