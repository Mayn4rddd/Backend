namespace backend.DTOs
{
    public class ResetPasswordDto
    {
        public string StudentId { get; set; }
        public string Name { get; set; }
        public string NewPassword { get; set; }
    }
}