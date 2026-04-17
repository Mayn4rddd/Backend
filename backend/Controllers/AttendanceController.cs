using backend.DTOs;
using backend.Enums;
using backend.Helpers;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        var now = TimeHelper.GetPhilippineTime();

        var student = _context.Students
            .FirstOrDefault(s => s.Id == dto.StudentId);

        if (student == null)
            return BadRequest("Student not found");

        // 1. Get assignment
        var assignment = _context.TeacherAssignments.FirstOrDefault(x =>
            x.TeacherId == dto.TeacherId &&
            x.SectionId == dto.SectionId &&
            x.SubjectId == dto.SubjectId
        );

        if (assignment == null)
            return BadRequest("Invalid assignment");

        // 2. Get schedule
        var schedule = _context.TeacherSchedules
            .FirstOrDefault(s =>
                s.TeacherAssignmentId == assignment.Id &&
                s.Day == now.DayOfWeek.ToString()
            );

        if (schedule == null)
            return BadRequest("No schedule today");

        var startTime = TimeSpan.Parse(schedule.StartTime);
        var endTime = TimeSpan.Parse(schedule.EndTime);
        var scheduleEnd = now.Date.Add(endTime);

        // 3. Block if class ended
        if (now > scheduleEnd)
            return BadRequest("Class time is over");

        // 🔥 4. GET ANY SESSION (QR OR MANUAL)
        var session = _context.AttendanceSessions
            .FirstOrDefault(s =>
                s.SectionId == dto.SectionId &&
                s.SubjectId == dto.SubjectId &&
                s.TeacherId == dto.TeacherId &&
                s.StartTime.Date == now.Date
            );

        if (session == null)
        {
            session = new AttendanceSession
            {
                SectionId = dto.SectionId,
                SubjectId = dto.SubjectId,
                TeacherId = dto.TeacherId,
                StartTime = now,
                Mode = "Manual"
            };

            _context.AttendanceSessions.Add(session);
            _context.SaveChanges();
        }

        // 🔥 5. CHECK IF ALREADY MARKED (QR OR MANUAL)
        var existingAttendance = _context.Attendance
            .FirstOrDefault(a =>
                a.StudentDbId == student.Id &&
                a.AttendanceSessionId == session.Id
            );

        if (existingAttendance != null)
        {
            if (existingAttendance.QrSessionId != null)
            {
                return BadRequest("Student already scanned via QR");
            }
            else
            {
                return BadRequest("Student already marked manually");
            }
        }

        // 6. Validate status
        var validStatuses = new[] { "Present", "Late", "Absent" };

        if (!validStatuses.Contains(dto.Status))
            return BadRequest("Invalid status");

        // 7. Save attendance
        var attendance = new Attendance
        {
            StudentDbId = student.Id,
            SectionId = dto.SectionId,
            SubjectId = dto.SubjectId,
            TeacherId = dto.TeacherId,
            Timestamp = now,
            Status = dto.Status,
            AttendanceSessionId = session.Id
        };

        _context.Attendance.Add(attendance);
        _context.SaveChanges();

        return Ok(new
        {
            message = "Manual attendance recorded",
            studentName = student.Name,
            status = dto.Status
        });
    }
    [HttpPost("scan")]
    public IActionResult Scan([FromBody] ScanDto dto)
    {
        var now = TimeHelper.GetPhilippineTime();

        var session = _context.QrSessions
            .FirstOrDefault(s => s.Token == dto.Token);

        if (session == null)
            return BadRequest("Invalid QR session");

        if (now > session.ScheduleEndTime)
            return BadRequest("Class time is over. QR is no longer valid.");

        var student = _context.Students
            .FirstOrDefault(s => s.StudentId == dto.StudentId);

        if (student == null)
            return BadRequest("Student not found");

        if (student.SectionId != session.SectionId)
        {
            return BadRequest("Invalid section for this QR");
        }

        var alreadyExists = _context.Attendance.Any(a =>
            a.StudentDbId == student.Id &&
            a.AttendanceSessionId == session.AttendanceSessionId
        );

        if (alreadyExists)
            return BadRequest("Already scanned for this session");

        string status;

        if (now <= session.Expiry)
        {
            status = AttendanceStatus.Present;
        }
        else
        {
            status = AttendanceStatus.Late;
        }

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

        try
        {
            _context.Attendance.Add(attendance);
            _context.SaveChanges();
        }
        catch (DbUpdateException)
        {
            return BadRequest("Already scanned for this session");
        }

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