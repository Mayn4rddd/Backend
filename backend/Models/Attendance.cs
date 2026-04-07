public class Attendance
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int SectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; }
}