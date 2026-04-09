namespace backend.DTOs;

public class GenerateQrDto
{
    public int UserId { get; set; }     // from logged in teacher
    public int SectionId { get; set; }  // selected section

    public int SubjectId { get; set; }   // 🔥 NEW
}