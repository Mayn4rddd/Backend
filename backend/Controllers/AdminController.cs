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

    // ✅ CREATE STUDENT
    [HttpPost("students")]
    public IActionResult CreateStudent([FromBody] CreateStudentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.StudentId) || string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("StudentId and Name are required");

        var exists = _context.Students
    .Where(s => s.StudentId != null)
    .FirstOrDefault(s => s.StudentId == dto.StudentId);
        if (exists != null)
            return BadRequest("Student already exists");

        var sectionExists = _context.Sections.Any(s => s.Id == dto.SectionId);
        if (!sectionExists)
            return BadRequest("Invalid SectionId");

        var student = new Student
        {
            StudentId = dto.StudentId,
            Name = dto.Name,
            SectionId = dto.SectionId,
            ParentPhone = dto.ParentPhone,
            IsRegistered = false
        };

        _context.Students.Add(student);
        _context.SaveChanges();

        return Ok(new { message = "Student created successfully" });
    }

    // ✅ CREATE CLASS
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

    // ✅ CREATE TEACHER (WITH USER)
    [HttpPost("create-teacher")]
    public IActionResult CreateTeacher([FromBody] CreateTeacherDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Name and Password required");

        // ✅ CREATE USER (LOGIN ACCOUNT)
        var user = new User
        {
            Name = dto.Name,
            Password = dto.Password,
            Role = "Teacher"
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        // ✅ CREATE TEACHER (PROFILE ONLY)
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

    // ✅ ASSIGN SUBJECT
    [HttpPost("assign-teacher-subject")]
    public IActionResult AssignTeacherSubject([FromBody] AssignTeacherSubjectDto dto)
    {
        var exists = _context.TeacherAssignments.FirstOrDefault(x =>
            x.TeacherId == dto.TeacherId &&
            x.SectionId == dto.SectionId &&   // ✅ FIXED
            x.SubjectId == dto.SubjectId);

        if (exists != null)
            return BadRequest("Already assigned");

        var assignment = new TeacherAssignment
        {
            TeacherId = dto.TeacherId,
            SectionId = dto.SectionId,   // ✅ FIXED
            SubjectId = dto.SubjectId
        };

        _context.TeacherAssignments.Add(assignment);
        _context.SaveChanges();

        return Ok(new { message = "Assigned successfully" });
    }

    // ✅ GET ALL SUBJECTS
    [HttpGet("subjects")]
    public IActionResult GetSubjects()
    {
        return Ok(_context.Subjects.ToList());
    }

    // ✅ GET ALL SECTIONS
    [HttpGet("sections")]
    public IActionResult GetSections()
    {
        return Ok(_context.Sections.ToList());
    }

    // ✅ GET ALL TEACHERS
    [HttpGet("teachers")]
    public IActionResult GetTeachers()
    {
        return Ok(_context.Teachers.ToList());
    }

    // ✅ GET ALL STUDENTS
    [HttpGet("students")]
    public IActionResult GetStudents()
    {
        return Ok(_context.Students.ToList());
    }

}
