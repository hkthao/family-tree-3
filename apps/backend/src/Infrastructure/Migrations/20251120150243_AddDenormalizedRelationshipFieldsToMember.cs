using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDenormalizedRelationshipFieldsToMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "father_full_name",
                table: "members",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "father_id",
                table: "members",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "husband_full_name",
                table: "members",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "husband_id",
                table: "members",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "mother_full_name",
                table: "members",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "mother_id",
                table: "members",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "wife_full_name",
                table: "members",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "wife_id",
                table: "members",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "father_full_name",
                table: "members");

            migrationBuilder.DropColumn(
                name: "father_id",
                table: "members");

            migrationBuilder.DropColumn(
                name: "husband_full_name",
                table: "members");

            migrationBuilder.DropColumn(
                name: "husband_id",
                table: "members");

            migrationBuilder.DropColumn(
                name: "mother_full_name",
                table: "members");

            migrationBuilder.DropColumn(
                name: "mother_id",
                table: "members");

            migrationBuilder.DropColumn(
                name: "wife_full_name",
                table: "members");

            migrationBuilder.DropColumn(
                name: "wife_id",
                table: "members");
        }
    }
}
