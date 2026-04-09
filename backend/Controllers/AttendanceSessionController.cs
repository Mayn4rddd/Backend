using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/session")]
public class AttendanceSessionController : ControllerBase
{
    private readonly AppDbContext _context;

    public AttendanceSessionController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("start")]
    public IActionResult StartSession([FromBody] StartSessionDto dto)
    {
        var session = new AttendanceSession
        {
            SectionId = dto.SectionId,
            SubjectId = dto.SubjectId,
            TeacherId = dto.TeacherId,
            StartTime = DateTime.UtcNow,
            Mode = dto.Mode // QR or Manual
        };

        _context.AttendanceSessions.Add(session);
        _context.SaveChanges();

        return Ok(session);
    }
}