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

    [HttpGet("my-classes")]
    public IActionResult GetMyClasses(int userId)
    {
        var teacher = _context.Teachers
            .FirstOrDefault(t => t.UserId == userId);

        if (teacher == null)
            return NotFound();

        var data = (from ta in _context.TeacherAssignments
                    join c in _context.Classes on ta.ClassId equals c.Id
                    join s in _context.Subjects on ta.SubjectId equals s.Id
                    where ta.TeacherId == teacher.Id
                    select new
                    {
                        className = c.Name,
                        subject = s.SubjectName
                    }).ToList();

        return Ok(data);
    }
}