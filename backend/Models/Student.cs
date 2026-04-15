namespace backend.Models;

public class Student
{
    public int Id { get; set; }
    public string StudentId { get; set; }
    public string Name { get; set; }
    public string ParentPhone { get; set; }

    public int SectionId { get; set; }   
    public bool IsRegistered { get; set; }

    public int? UserId { get; set; }
    public User User { get; set; }

    public Section Section { get; set; }
}