using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveResizedImageUrlFromMemberStoryImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_member_story_image_member_stories_member_story_id",
                table: "member_story_image");

            migrationBuilder.DropPrimaryKey(
                name: "pk_member_story_image",
                table: "member_story_image");

            migrationBuilder.DropColumn(
                name: "caption",
                table: "member_story_image");

            migrationBuilder.DropColumn(
                name: "resized_image_url",
                table: "member_story_image");

            migrationBuilder.RenameTable(
                name: "member_story_image",
                newName: "member_story_images");

            migrationBuilder.RenameIndex(
                name: "ix_member_story_image_member_story_id",
                table: "member_story_images",
                newName: "ix_member_story_images_member_story_id");

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

            migrationBuilder.AddPrimaryKey(
                name: "pk_member_story_images",
                table: "member_story_images",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_member_story_images_member_stories_member_story_id",
                table: "member_story_images",
                column: "member_story_id",
                principalTable: "member_stories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_member_story_images_member_stories_member_story_id",
                table: "member_story_images");

            migrationBuilder.DropPrimaryKey(
                name: "pk_member_story_images",
                table: "member_story_images");

            migrationBuilder.RenameTable(
                name: "member_story_images",
                newName: "member_story_image");

            migrationBuilder.RenameIndex(
                name: "ix_member_story_images_member_story_id",
                table: "member_story_image",
                newName: "ix_member_story_image_member_story_id");

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

            migrationBuilder.AddColumn<string>(
                name: "caption",
                table: "member_story_image",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "resized_image_url",
                table: "member_story_image",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "pk_member_story_image",
                table: "member_story_image",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_member_story_image_member_stories_member_story_id",
                table: "member_story_image",
                column: "member_story_id",
                principalTable: "member_stories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
