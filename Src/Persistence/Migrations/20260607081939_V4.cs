using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanRegister",
                table: "Exam",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanShowResults",
                table: "Exam",
                type: "bit",
                nullable: false,
                defaultValue: false);


            migrationBuilder.Sql(@"
                UPDATE Exam 
                SET PIN = PIN * 100 + CAST(R.Val AS INT)
                FROM Exam
                CROSS APPLY
                (SELECT Val = ABS (CHECKSUM (NEWID ())) % 100) AS R;
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanRegister",
                table: "Exam");

            migrationBuilder.DropColumn(
                name: "CanShowResults",
                table: "Exam");
        }
    }
}
