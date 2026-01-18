using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameVoiceTablesToPlural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_voice_generation_voice_profile_voice_profile_id",
                table: "voice_generation");

            migrationBuilder.DropForeignKey(
                name: "fk_voice_profile_members_member_id",
                table: "voice_profile");

            migrationBuilder.DropPrimaryKey(
                name: "pk_voice_profile",
                table: "voice_profile");

            migrationBuilder.DropPrimaryKey(
                name: "pk_voice_generation",
                table: "voice_generation");

            migrationBuilder.RenameTable(
                name: "voice_profile",
                newName: "voice_profiles");

            migrationBuilder.RenameTable(
                name: "voice_generation",
                newName: "voice_generations");

            migrationBuilder.RenameIndex(
                name: "ix_voice_profile_member_id",
                table: "voice_profiles",
                newName: "ix_voice_profiles_member_id");

            migrationBuilder.RenameIndex(
                name: "ix_voice_generation_voice_profile_id",
                table: "voice_generations",
                newName: "ix_voice_generations_voice_profile_id");

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
                name: "pk_voice_profiles",
                table: "voice_profiles",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_voice_generations",
                table: "voice_generations",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_voice_generations_voice_profiles_voice_profile_id",
                table: "voice_generations",
                column: "voice_profile_id",
                principalTable: "voice_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_voice_profiles_members_member_id",
                table: "voice_profiles",
                column: "member_id",
                principalTable: "members",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_voice_generations_voice_profiles_voice_profile_id",
                table: "voice_generations");

            migrationBuilder.DropForeignKey(
                name: "fk_voice_profiles_members_member_id",
                table: "voice_profiles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_voice_profiles",
                table: "voice_profiles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_voice_generations",
                table: "voice_generations");

            migrationBuilder.RenameTable(
                name: "voice_profiles",
                newName: "voice_profile");

            migrationBuilder.RenameTable(
                name: "voice_generations",
                newName: "voice_generation");

            migrationBuilder.RenameIndex(
                name: "ix_voice_profiles_member_id",
                table: "voice_profile",
                newName: "ix_voice_profile_member_id");

            migrationBuilder.RenameIndex(
                name: "ix_voice_generations_voice_profile_id",
                table: "voice_generation",
                newName: "ix_voice_generation_voice_profile_id");

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

            migrationBuilder.AddPrimaryKey(
                name: "pk_voice_profile",
                table: "voice_profile",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_voice_generation",
                table: "voice_generation",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_voice_generation_voice_profile_voice_profile_id",
                table: "voice_generation",
                column: "voice_profile_id",
                principalTable: "voice_profile",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_voice_profile_members_member_id",
                table: "voice_profile",
                column: "member_id",
                principalTable: "members",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
