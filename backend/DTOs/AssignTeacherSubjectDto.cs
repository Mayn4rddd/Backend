
using System.Collections.Generic;

namespace backend.DTOs;
public class AssignTeacherSubjectDto
{
    public int TeacherId { get; set; }
    public int SectionId { get; set; }   
    public int SubjectId { get; set; }

    public List<ScheduleDto> Schedules { get; set; } = new();
}