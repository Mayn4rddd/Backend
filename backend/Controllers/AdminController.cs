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