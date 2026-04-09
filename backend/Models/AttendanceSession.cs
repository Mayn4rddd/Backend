namespace backend.Models;

public class AttendanceSession
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public int SubjectId { get; set; }
    public int TeacherId { get; set; }
    public DateTime StartTime { get; set; }

    public string Mode { get; set; } // "QR" or "Manual"
}