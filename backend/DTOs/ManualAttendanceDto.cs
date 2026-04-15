namespace backend.DTOs;

public class ManualAttendanceDto
{
    public int SessionId { get; set; }
    public int StudentId { get; set; }
    public int SectionId { get; set; }
    public int SubjectId { get; set; }
    public int TeacherId { get; set; }
    public string Status { get; set; } 
}