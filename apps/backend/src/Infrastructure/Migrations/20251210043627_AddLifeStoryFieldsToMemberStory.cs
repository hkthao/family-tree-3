using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLifeStoryFieldsToMemberStory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "original_image_url",
                table: "member_stories");

            migrationBuilder.DropColumn(
                name: "perspective",
                table: "member_stories");

            migrationBuilder.DropColumn(
                name: "raw_input",
                table: "member_stories");

            migrationBuilder.RenameColumn(
                name: "story_style",
                table: "member_stories",
                newName: "time_range_description");

            migrationBuilder.RenameColumn(
                name: "resized_image_url",
                table: "member_stories",
                newName: "location");

            migrationBuilder.AddColumn<int>(
                name: "certainty_level",
                table: "member_stories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_year_estimated",
                table: "member_stories",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "life_stage",
                table: "member_stories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "storyteller_id",
                table: "member_stories",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "year",
                table: "member_stories",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "family_dicts",
                type: "char(50)",
                maxLength: 50,
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(string),
                oldType: "char(50)",
                oldMaxLength: 50)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "member_story_image",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    member_story_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    image_url = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    resized_image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    caption = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    deleted_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    deleted_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    last_modified_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_member_story_image", x => x.id);
                    table.ForeignKey(
                        name: "fk_member_story_image_member_stories_member_story_id",
                        column: x => x.member_story_id,
                        principalTable: "member_stories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_member_story_image_member_story_id",
                table: "member_story_image",
                column: "member_story_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "member_story_image");

            migrationBuilder.DropColumn(
                name: "certainty_level",
                table: "member_stories");

            migrationBuilder.DropColumn(
                name: "is_year_estimated",
                table: "member_stories");

            migrationBuilder.DropColumn(
                name: "life_stage",
                table: "member_stories");

            migrationBuilder.DropColumn(
                name: "storyteller_id",
                table: "member_stories");

            migrationBuilder.DropColumn(
                name: "year",
                table: "member_stories");

            migrationBuilder.RenameColumn(
                name: "time_range_description",
                table: "member_stories",
                newName: "story_style");

            migrationBuilder.RenameColumn(
                name: "location",
                table: "member_stories",
                newName: "resized_image_url");

            migrationBuilder.AddColumn<string>(
                name: "original_image_url",
                table: "member_stories",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "perspective",
                table: "member_stories",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "raw_input",
                table: "member_stories",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "family_dicts",
                type: "char(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }
    }
}
