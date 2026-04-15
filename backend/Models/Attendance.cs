namespace backend.Models;

public class Attendance
{
    public int Id { get; set; }
    public int StudentDbId { get; set; }
    public Student Student { get; set; }
    public int SectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; }
    public int TeacherId { get; set; }  
    public int SubjectId { get; set; }   

    public int QrSessionId { get; set; }

    public int AttendanceSessionId { get; set; }

}