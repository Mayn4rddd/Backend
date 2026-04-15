namespace backend.Models
{
    public class TeacherSchedule
    {
        public int Id { get; set; }

        public int TeacherAssignmentId { get; set; }

        public string Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
