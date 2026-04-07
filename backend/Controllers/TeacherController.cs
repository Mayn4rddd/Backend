using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
    public IActionResult Export(int sectionId)
    {
        var students = _context.Students
            .Where(s => s.SectionId == sectionId)
            .ToList();

        var attendance = _context.Attendance
            .Where(a => a.SectionId == sectionId)
            .ToList();

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance");

        worksheet.Cell(1, 1).Value = "Name";
        worksheet.Cell(1, 2).Value = "Status";
        worksheet.Cell(1, 3).Value = "Date";

        int row = 2;

        foreach (var student in students)
        {
            var att = attendance.FirstOrDefault(a => a.StudentId == student.Id);

            worksheet.Cell(row, 1).Value = student.Name;
            worksheet.Cell(row, 2).Value = att != null ? "Present" : "Absent";
            worksheet.Cell(row, 3).Value = att?.Timestamp;

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

    [HttpGet("my-students")]
    public IActionResult GetMyStudents([FromQuery] int userId, [FromQuery] int sectionId)
    {
        var teacher = _context.Teachers
            .FirstOrDefault(t => t.UserId == userId);

        if (teacher == null)
            return NotFound("Teacher not found");

        var today = DateTime.Today;

        // ✅ ALL STUDENTS
        var allStudents = _context.Students
            .Where(s => s.SectionId == sectionId)
            .Select(s => new
            {
                studentId = s.Id,
                studentName = s.Name
            })
            .ToList();

        // ✅ PRESENT STUDENTS (scanned today)
        var presentStudents = _context.Attendance
            .Where(a => a.SectionId == sectionId && a.Timestamp.Date == today)
            .Select(a => new
            {
                studentId = a.StudentId,
                status = a.Status,
                date = a.Timestamp
            })
            .ToList();

        // ✅ COMBINE → ADD STATUS
        var result = allStudents.Select(s =>
        {
            var present = presentStudents
                .FirstOrDefault(p => p.studentId == s.studentId);

            return new
            {
                studentId = s.studentId,
                studentName = s.studentName,
                status = present != null ? "Present" : "Absent",
                date = present != null ? present.date : (DateTime?)null
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
                        subject = s.Name
                    }).Distinct().ToList();

        return Ok(data);
    }
}

