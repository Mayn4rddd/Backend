using Microsoft.AspNetCore.Mvc;

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
    public IActionResult CreateStudent(Student student)
    {

        student.IsRegistered = false;
        _context.Students.Add(student);
        _context.SaveChanges();
        return Ok(student);
    }


    [HttpPost("classes")]
    public IActionResult CreateClass(Class cls)
    {
        _context.Classes.Add(cls);
        _context.SaveChanges();
        return Ok(cls);
    }
    [HttpPost("assign-teacher-subject")]
    public IActionResult AssignTeacherSubject(int teacherId, int classId, int subjectId)
    {
        // check if exists
        var exists = _context.TeacherAssignments
            .FirstOrDefault(x =>
                x.TeacherId == teacherId &&
                x.ClassId == classId &&
                x.SubjectId == subjectId);

        if (exists != null)
            return BadRequest("Already assigned");

        var assignment = new TeacherAssignment
        {
            TeacherId = teacherId,
            ClassId = classId,
            SubjectId = subjectId
        };

        _context.TeacherAssignments.Add(assignment);
        _context.SaveChanges();

        return Ok("Assigned successfully");
    }

    [HttpPost("create-teacher")]
    public IActionResult CreateTeacher(string name, string password)
    {
        var user = new User
        {
            Name = name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Teacher"
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        var teacher = new Teacher
        {
            Name = name,
            UserId = user.Id
        };

        _context.Teachers.Add(teacher);
        _context.SaveChanges();

        return Ok("Teacher created");
    }
    [HttpPost("assign-teacher")]
    public IActionResult AssignTeacher(int classId, int teacherId)
    {
        var cls = _context.Classes.Find(classId);
        if (cls == null) return NotFound();

        cls.TeacherId = teacherId;
        _context.SaveChanges();

        return Ok();
    }
}

