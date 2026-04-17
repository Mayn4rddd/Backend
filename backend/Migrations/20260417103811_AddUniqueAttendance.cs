using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attendance_StudentDbId",
                table: "Attendance");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_StudentDbId_AttendanceSessionId",
                table: "Attendance",
                columns: new[] { "StudentDbId", "AttendanceSessionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attendance_StudentDbId_AttendanceSessionId",
                table: "Attendance");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_StudentDbId",
                table: "Attendance",
                column: "StudentDbId");
        }
    }
}
