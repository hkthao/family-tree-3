using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Members",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Families",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Events",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Members_Code",
                table: "Members",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Families_Code",
                table: "Families",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_Code",
                table: "Events",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_Code",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Families_Code",
                table: "Families");

            migrationBuilder.DropIndex(
                name: "IX_Events_Code",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Events");
        }
    }
}
