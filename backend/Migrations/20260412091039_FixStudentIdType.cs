using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixStudentIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Attendance",
                newName: "StudentDbId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_StudentDbId",
                table: "Attendance",
                column: "StudentDbId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Students_StudentDbId",
                table: "Attendance",
                column: "StudentDbId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Students_StudentDbId",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_StudentDbId",
                table: "Attendance");

            migrationBuilder.RenameColumn(
                name: "StudentDbId",
                table: "Attendance",
                newName: "StudentId");
        }
    }
}
