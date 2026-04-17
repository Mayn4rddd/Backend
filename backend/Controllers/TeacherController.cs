using backend.DTOs;
using backend.Helpers;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/teacher")]
public class TeacherController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeacherController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("export")]
    public IActionResult Export(int sectionId, int subjectId, int teacherId)
    {
        var students = _context.Students
            .Where(s => s.SectionId == sectionId)
            .ToList();

        var today = TimeHelper.GetPhilippineTime().Date;

        var attendance = _context.Attendance    
            .Where(a =>
                a.SectionId == sectionId &&
                a.SubjectId == subjectId &&
                a.TeacherId == teacherId &&
                a.Timestamp.Date == today
            )
            .ToList();

        var studentDict = students.ToDictionary(s => s.Id);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance");

        worksheet.Cell(1, 1).Value = "Name";
        worksheet.Cell(1, 2).Value = "Status";
        worksheet.Cell(1, 3).Value = "Date";

        int row = 2;

        foreach (var att in attendance)
        {
            var student = studentDict.ContainsKey(att.StudentDbId)
                ? studentDict[att.StudentDbId]
                : null;

            worksheet.Cell(row, 1).Value = student?.Name;
            worksheet.Cell(row, 2).Value = att.Status;
            worksheet.Cell(row, 3).Value = att.Timestamp;

            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "attendance.xlsx"
        );
    }

    [HttpGet("masterlist-grid")]
    public IActionResult GetMasterlistGrid(
     int sectionId,
     int subjectId,
     int teacherId,
     DateTime startDate,
     DateTime endDate)
    {
        var students = _context.Students
            .Where(s => s.SectionId == sectionId)
            .OrderBy(s => s.Name)
            .ToList();

        var assignmentIds = _context.TeacherAssignments
    .Where(ta =>
        ta.SectionId == sectionId &&
        ta.SubjectId == subjectId &&
        ta.TeacherId == teacherId
    )
    .Select(ta => ta.Id)
    .ToList();

        var scheduleDays = _context.TeacherSchedules
            .Where(ts => assignmentIds.Contains(ts.TeacherAssignmentId))
            .Select(ts => ts.Day)
            .Distinct()
            .ToList();


        var dates = Enumerable.Range(0, (endDate - startDate).Days + 1)
            .Select(d => startDate.AddDays(d))
            .Where(date => scheduleDays.Contains(date.DayOfWeek.ToString()))
            .Select(date => date.ToString("yyyy-MM-dd"))
            .ToList();

        var attendance = _context.Attendance
            .Where(a =>
                a.SectionId == sectionId &&
                a.SubjectId == subjectId &&
                a.TeacherId == teacherId &&
                a.Timestamp.Date >= startDate.Date &&
                a.Timestamp.Date <= endDate.Date
            )
            .ToList();

        var result = students.Select(s =>
        {
            var records = new Dictionary<string, string>();

            foreach (var date in dates)
            {
                var parsedDate = DateTime.Parse(date);

                var att = attendance
                    .Where(a =>
                        a.StudentDbId == s.Id &&
                        a.Timestamp.Date == parsedDate.Date
                    )
                    .OrderByDescending(a => a.Timestamp)
                    .FirstOrDefault();

                var isScheduledDay = scheduleDays.Contains(parsedDate.DayOfWeek.ToString());

                if (!isScheduledDay)
                {
                    records[date] = ""; 
                }
                else if (att != null)
                {
                    records[date] = att.Status == "Present" ? "P"
                        : att.Status == "Late" ? "L"
                        : "A";
                }
                else
                {
                    records[date] = "";
                }
            }

            return new
            {
                studentId = s.StudentId,
                studentName = s.Name,
                records = records
            };
        }).ToList();

        return Ok(new
        {
            dates,
            students = result
        });
    }
    [HttpGet("masterlist")]
    public IActionResult GetMasterlist(int sectionId, int subjectId, int teacherId)
    {
        var students = _context.Students
            .Where(s => s.SectionId == sectionId)
            .ToList();

        var attendanceRecords = _context.Attendance
            .Where(a =>
                a.SectionId == sectionId &&
                a.SubjectId == subjectId &&
                a.TeacherId == teacherId
            )
            .ToList();

        var result = students.Select(s =>
        {
            var studentAttendance = attendanceRecords
                .Where(a => a.StudentDbId == s.Id)
                .ToList();

            var attendanceByDate = studentAttendance
                .GroupBy(a => a.Timestamp.Date)
                .ToDictionary(
                    g => g.Key.ToString("yyyy-MM-dd"),
                    g => g.OrderByDescending(x => x.Timestamp).First().Status
                );

            return new
            {
                id = s.Id,
                studentId = s.StudentId,
                studentName = s.Name,
                attendance = attendanceByDate
            };
        }).ToList();

        var section = _context.Sections.FirstOrDefault(s => s.Id == sectionId);

        return Ok(new
        {
            sectionName = section?.Name,
            students = result
        });
    }

    [HttpGet("attendance-summary")]
    public IActionResult GetAttendanceSummary(int sectionId, int subjectId)
    {
        var today = TimeHelper.GetPhilippineTime().Date;

        var totalStudents = _context.Students
            .Count(s => s.SectionId == sectionId);

        var todayAttendance = _context.Attendance
            .Where(a =>
                a.SectionId == sectionId &&
                a.SubjectId == subjectId &&
                a.Timestamp.Date == today
            )
            .ToList();

        var present = todayAttendance.Count(a => a.Status == "Present");
        var late = todayAttendance.Count(a => a.Status == "Late");

        var marked = present + late;
        var absent = totalStudents - marked;

        double percentage = totalStudents == 0
            ? 0
            : (double)marked / totalStudents * 100;

        return Ok(new
        {
            totalStudents,
            present,
            late,
            absent,
            percentage = Math.Round(percentage, 2)
        });
    }

    [HttpGet("my-students")]
    public IActionResult GetMyStudents(
     [FromQuery] int userId,
     [FromQuery] int sectionId,
     [FromQuery] int subjectId
 )
    {
        var teacher = _context.Teachers
            .FirstOrDefault(t => t.UserId == userId);

        if (teacher == null)
            return NotFound("Teacher not found");

        var today = TimeHelper.GetPhilippineTime().Date;

        var allStudents = _context.Students
            .Where(s => s.SectionId == sectionId)
            .Select(s => new
            {
                studentId = s.Id,
                studentName = s.Name
            })
            .ToList();

        var presentStudents = _context.Attendance
            .Where(a =>
                a.SectionId == sectionId &&
                a.SubjectId == subjectId &&
                a.TeacherId == teacher.Id &&
                a.Timestamp.Date == today
            )
            .Select(a => new
            {
                studentId = a.StudentDbId,
                status = a.Status,
                date = a.Timestamp
            })
            .ToList();

        var result = allStudents.Select(s =>
        {
            var present = presentStudents
                .FirstOrDefault(p => p.studentId == s.studentId);

            return new
            {
                studentId = s.studentId,
                studentName = s.studentName,
                status = present != null ? present.status : "Absent",
                date = present?.date
            };
        }).ToList();

        return Ok(result);
    }

    [HttpGet("my-sections")]
    public IActionResult GetMySections(int userId)
    {
        var teacher = _context.Teachers
            .FirstOrDefault(t => t.UserId == userId);

        if (teacher == null)
            return NotFound("Teacher not found");

        var data = (from ta in _context.TeacherAssignments
                    join sec in _context.Sections on ta.SectionId equals sec.Id
                    join s in _context.Subjects on ta.SubjectId equals s.Id
                    where ta.TeacherId == teacher.Id
                    select new
                    {
                        sectionId = sec.Id,
                        section = sec.Name,
                        subjectId = s.Id,
                        subject = s.Name,
                        teacherId = ta.TeacherId, 
                        schedules = _context.TeacherSchedules
                            .Where(ts => ts.TeacherAssignmentId == ta.Id)
                            .Select(ts => new
                            {   
                                day = ts.Day,
                                startTime = ts.StartTime,
                                endTime = ts.EndTime
                            }).ToList()
                    }).ToList();

        return Ok(data);
    }

    [HttpGet("session-attendance")]
    public IActionResult GetSessionAttendance(int sessionId)
    {
        var attendance = _context.Attendance
            .Include(a => a.Student)
            .Where(a => a.AttendanceSessionId == sessionId)
            .Select(a => new AttendanceViewDto
            {
                StudentName = a.Student.Name,
                Status = a.Status,
                Time = a.Timestamp
            })
            .ToList();

        return Ok(attendance);
    }
}