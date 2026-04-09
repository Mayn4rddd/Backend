namespace backend.Models;

public class Attendance
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int SectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; }
    public int TeacherId { get; set; }   // 🔥 ADD THIS
    public int SubjectId { get; set; }   // 🔥 NEW

    public int QrSessionId { get; set; }

    public int AttendanceSessionId { get; set; }

}