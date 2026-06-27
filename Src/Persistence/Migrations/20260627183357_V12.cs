using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentExam_RegistrationCode_ExamId",
                table: "StudentExam");

            migrationBuilder.AlterColumn<string>(
                name: "RegistrationCode",
                table: "StudentExam",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "StudentExam",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "StudentCourse",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentExam_RegistrationCode_ExamId",
                table: "StudentExam",
                columns: new[] { "RegistrationCode", "ExamId" },
                unique: true,
                filter: "[RegistrationCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentExam_RegistrationCode_ExamId",
                table: "StudentExam");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "StudentExam");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "StudentCourse");

            migrationBuilder.AlterColumn<string>(
                name: "RegistrationCode",
                table: "StudentExam",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentExam_RegistrationCode_ExamId",
                table: "StudentExam",
                columns: new[] { "RegistrationCode", "ExamId" },
                unique: true);
        }
    }
}
