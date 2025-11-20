using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDenormalizedAvatarFieldsToMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "father_avatar_url",
                table: "members",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "husband_avatar_url",
                table: "members",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "mother_avatar_url",
                table: "members",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "wife_avatar_url",
                table: "members",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "father_avatar_url",
                table: "members");

            migrationBuilder.DropColumn(
                name: "husband_avatar_url",
                table: "members");

            migrationBuilder.DropColumn(
                name: "mother_avatar_url",
                table: "members");

            migrationBuilder.DropColumn(
                name: "wife_avatar_url",
                table: "members");
        }
    }
}
