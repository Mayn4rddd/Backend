namespace backend.Models;

public class QrSession
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public int TeacherId { get; set; }
    public string Token { get; set; }
    public DateTime Expiry { get; set; }

    public int SubjectId { get; set; }   

    public DateTime StartTime { get; set; }   

    public int AttendanceSessionId { get; set; }

}