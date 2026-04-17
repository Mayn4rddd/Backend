using backend.DTOs;
using backend.Helpers;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

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
            StartTime = TimeHelper.GetPhilippineTime(),
            Mode = dto.Mode 
        };

        _context.AttendanceSessions.Add(session);
        _context.SaveChanges();

        return Ok(session);
    }
}