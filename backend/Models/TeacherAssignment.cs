namespace backend.Models;
public class TeacherAssignment
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public int SectionId { get; set; }
    public int SubjectId { get; set; }
}