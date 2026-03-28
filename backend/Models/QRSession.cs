public class QrSession
{
    public int Id { get; set; }
    public Guid SessionId { get; set; }
    public int ClassId { get; set; }
    public int TeacherId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryTime { get; set; }
    public bool IsActive { get; set; }
}