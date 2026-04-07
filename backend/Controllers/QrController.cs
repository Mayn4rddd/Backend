using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/qr")]
public class QrController : ControllerBase
{
    [HttpPost("generate")]
    public IActionResult GenerateQR()
    {
        // temporary sample response
        return Ok(new
        {
            sessionId = Guid.NewGuid(),
            token = Guid.NewGuid().ToString(),
            expiresAt = DateTime.UtcNow.AddMinutes(5)
        });
    }
}