using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Schedule",
                table: "TeacherAssignments");

            migrationBuilder.AddColumn<string>(
                name: "Day",
                table: "TeacherAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndTime",
                table: "TeacherAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartTime",
                table: "TeacherAssignments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day",
                table: "TeacherAssignments");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "TeacherAssignments");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "TeacherAssignments");

            migrationBuilder.AddColumn<string>(
                name: "Schedule",
                table: "TeacherAssignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
