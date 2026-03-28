public class Class
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? TeacherId { get; set; }

    public Teacher Teacher { get; set; }
}