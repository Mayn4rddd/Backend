using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("students")]
    public IActionResult CreateStudent([FromBody] CreateStudentDto dto)
    {
        if (dto == null)
            return BadRequest("Invalid data");

        if (string.IsNullOrWhiteSpace(dto.StudentId) || string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("StudentId and Name are required");

        var exists = _context.Students
            .Any(s => s.StudentId == dto.StudentId);

        if (exists)
            return BadRequest("Student already exists");

        var sectionExists = _context.Sections
            .Any(s => s.Id == dto.SectionId);

        if (!sectionExists)
            return BadRequest("Invalid SectionId");

        var student = new Student
        {
            StudentId = dto.StudentId.Trim(),
            Name = dto.Name.Trim(),
            SectionId = dto.SectionId,
            ParentPhone = dto.ParentPhone,
            IsRegistered = false
        };

        _context.Students.Add(student);
        _context.SaveChanges();

        return Ok(new
        {
            message = "Student created successfully",
            studentId = student.Id
        });
    }

    [HttpPost("create-section")]
    public IActionResult CreateSection([FromBody] CreateSectionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Section name is required");

        var exists = _context.Sections.Any(s => s.Name == dto.Name);
        if (exists)
            return BadRequest("Section already exists");

        _context.Sections.Add(new Section { Name = dto.Name });
        _context.SaveChanges();

        return Ok(new { message = "Section created successfully" });
    }

    [HttpPost("create-teacher")]
    public IActionResult CreateTeacher([FromBody] CreateTeacherDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Name and Password required");

        var user = new User
        {
            Name = dto.Name,
            Username = dto.Username,
            Password = dto.Password,
            Role = "Teacher"
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        var teacher = new Teacher
        {
            Name = dto.Name,
            UserId = user.Id
        };

        _context.Teachers.Add(teacher);
        _context.SaveChanges();

        return Ok(new { message = "Teacher created successfully" });
    }

    [HttpPost("create-subject")]
    public IActionResult CreateSubject([FromBody] CreateSubjectDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Subject name is required");

        var exists = _context.Subjects.Any(s => s.Name == dto.Name);
        if (exists)
            return BadRequest("Subject already exists");

        var subject = new Subject
        {
            Name = dto.Name
        };

        _context.Subjects.Add(subject);
        _context.SaveChanges();

        return Ok(new { message = "Subject created successfully", subject });
    }

    [HttpPost("assign-teacher-subject")]
    public IActionResult AssignTeacherSubject([FromBody] AssignTeacherSubjectDto dto)
    {
        if (dto == null)
            return BadRequest("Invalid data");

        if (dto.TeacherId <= 0 || dto.SectionId <= 0 || dto.SubjectId <= 0)
            return BadRequest("Invalid IDs");

        if (dto.Schedules == null || !dto.Schedules.Any())
            return BadRequest("At least one schedule is required");

        var exists = _context.TeacherAssignments.FirstOrDefault(x =>
            x.TeacherId == dto.TeacherId &&
            x.SectionId == dto.SectionId &&
            x.SubjectId == dto.SubjectId);

        if (exists != null)
            return BadRequest("Already assigned");

        var assignment = new TeacherAssignment
        {
            TeacherId = dto.TeacherId,
            SectionId = dto.SectionId,
            SubjectId = dto.SubjectId
        };

        _context.TeacherAssignments.Add(assignment);
        _context.SaveChanges();

        foreach (var sched in dto.Schedules)
        {
            if (string.IsNullOrWhiteSpace(sched.StartTime) ||
                string.IsNullOrWhiteSpace(sched.EndTime))
            {
                return BadRequest("Time is required");
            }

            var schedule = new TeacherSchedule
            {
                TeacherAssignmentId = assignment.Id,
                Day = sched.Day,
                StartTime = sched.StartTime,
                EndTime = sched.EndTime
            };

            _context.TeacherSchedules.Add(schedule);
        }

        _context.SaveChanges();

        return Ok(new { message = "Assigned with schedules" });
    }

    [HttpGet("subjects")]
    public IActionResult GetSubjects()
    {
        return Ok(_context.Subjects.ToList());
    }

    [HttpDelete("subject/{id}")]
    public IActionResult DeleteSubject(int id, [FromQuery] bool force = false)
    {
        var subject = _context.Subjects.Find(id);

        if (subject == null)
            return NotFound("Subject not found");

        var assignments = _context.TeacherAssignments
            .Where(t => t.SubjectId == id)
            .ToList();

        if (!force && assignments.Any())
        {
            return BadRequest("Subject has assignments. Use force delete.");
        }

        if (force)
        {
            _context.TeacherAssignments.RemoveRange(assignments);
        }

        _context.Subjects.Remove(subject);
        _context.SaveChanges();

        return Ok(new
        {
            message = force
                ? "Subject and assignments deleted"
                : "Subject deleted"
        });
    }

    [HttpPut("subject/{id}")]
    public IActionResult UpdateSubject(int id, [FromBody] CreateSubjectDto dto)
    {
        var subject = _context.Subjects.Find(id);

        if (subject == null)
            return NotFound("Subject not found");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Subject name is required");

        var exists = _context.Subjects
            .Any(s => s.Name == dto.Name && s.Id != id);

        if (exists)
            return BadRequest("Subject already exists");

        subject.Name = dto.Name;

        _context.SaveChanges();

        return Ok(new { message = "Subject updated successfully", subject });
    }

    [HttpGet("sections")]
    public IActionResult GetSections()
    {
        return Ok(_context.Sections.ToList());
    }

    [HttpDelete("section/{id}")]
    public IActionResult DeleteSection(int id)
    {
        var section = _context.Sections.Find(id);

        if (section == null)
            return NotFound("Section not found");

        _context.Sections.Remove(section);
        _context.SaveChanges();

        return Ok(new { message = "Section deleted successfully" });
    }

    [HttpPut("section/{id}")]
    public IActionResult UpdateSection(int id, [FromBody] CreateSectionDto dto)
    {
        var section = _context.Sections.Find(id);

        if (section == null)
            return NotFound("Section not found");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Section name is required");

        var exists = _context.Sections
            .Any(s => s.Name == dto.Name && s.Id != id);

        if (exists)
            return BadRequest("Section already exists");

        section.Name = dto.Name;

        _context.SaveChanges();

        return Ok(new { message = "Section updated successfully", section });
    }

    [HttpGet("teachers")]
    public IActionResult GetTeachers()
    {
        return Ok(_context.Teachers.ToList());
    }

    [HttpPut("teacher/{id}")]
    public IActionResult UpdateTeacher(int id, [FromBody] CreateTeacherDto dto)
    {
        var teacher = _context.Teachers.Find(id);

        if (teacher == null)
            return NotFound("Teacher not found");

        teacher.Name = dto.Name;

        var user = _context.Users.FirstOrDefault(u => u.Id == teacher.UserId);

        if (user != null)
        {
            user.Name = dto.Name;
            user.Username = dto.Username;
            user.Password = dto.Password;
        }

        _context.SaveChanges();

        return Ok(new { message = "Teacher updated successfully" });
    }

    [HttpDelete("teacher/{id}")]
    public IActionResult DeleteTeacher(int id)
    {
        var teacher = _context.Teachers.Find(id);

        if (teacher == null)
            return NotFound("Teacher not found");

        var user = _context.Users.FirstOrDefault(u => u.Id == teacher.UserId);

        _context.Teachers.Remove(teacher);

        if (user != null)
            _context.Users.Remove(user);

        _context.SaveChanges();

        return Ok(new { message = "Teacher deleted successfully" });
    }

    [HttpGet("students")]
    public IActionResult GetStudents()
    {
        return Ok(_context.Students.ToList());
    }

    [HttpPut("student/{id}")]
    public IActionResult UpdateStudent(int id, [FromBody] CreateStudentDto dto)
    {
        var student = _context.Students.Find(id);

        if (student == null)
            return NotFound("Student not found");

        student.Name = dto.Name;
        student.StudentId = dto.StudentId;
        student.SectionId = dto.SectionId;
        student.ParentPhone = dto.ParentPhone;

        _context.SaveChanges();

        return Ok(new { message = "Student updated successfully" });
    }

    [HttpDelete("student/{id}")]
    public IActionResult DeleteStudent(int id)
    {
        var student = _context.Students.Find(id);

        if (student == null)
            return NotFound("Student not found");

        _context.Students.Remove(student);
        _context.SaveChanges();

        return Ok(new { message = "Student deleted successfully" });
    }
}