using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeycloakUserId",
                table: "Teacher",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teacher_KeycloakUserId",
                table: "Teacher",
                column: "KeycloakUserId",
                unique: true,
                filter: "[KeycloakUserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teacher_KeycloakUserId",
                table: "Teacher");

            migrationBuilder.DropColumn(
                name: "KeycloakUserId",
                table: "Teacher");
        }
    }
}
