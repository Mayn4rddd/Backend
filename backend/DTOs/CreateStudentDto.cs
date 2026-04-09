namespace backend.DTOs;

public class CreateStudentDto
{
    public string StudentId { get; set; }
    public string Name { get; set; }
    public int SectionId { get; set; }
    public string ParentPhone { get; set; }
}