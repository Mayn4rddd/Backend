public class AssignTeacherSubjectDto
{
    public int TeacherId { get; set; }
    public int SectionId { get; set; }   // ✅ NEW
    public int SubjectId { get; set; }
}