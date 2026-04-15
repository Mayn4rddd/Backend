namespace backend.DTOs;

public class StartSessionDto
{
    public int SectionId { get; set; }
    public int SubjectId { get; set; }
    public int TeacherId { get; set; }
    public string Mode { get; set; }
}