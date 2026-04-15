namespace backend.Models
{
    public class Schedule
    {
        public int Id { get; set; }

        public int SectionId { get; set; }
        public int SubjectId { get; set; }

        public string Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
