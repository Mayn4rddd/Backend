using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


using backend.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<TeacherAssignment> TeacherAssignments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<TeacherSchedule> TeacherSchedules { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Attendance> Attendance { get; set; }
    public DbSet<QrSession> QrSessions { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentDbId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Attendance>()
    .HasIndex(a => new { a.StudentDbId, a.AttendanceSessionId })
    .IsUnique();
    }


    public DbSet<AttendanceSession> AttendanceSessions { get; set; }
}