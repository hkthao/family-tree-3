using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImageRestorationJobFamilyIdAndRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "family_id",
                table: "image_restoration_jobs",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

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

            migrationBuilder.CreateIndex(
                name: "ix_image_restoration_jobs_family_id",
                table: "image_restoration_jobs",
                column: "family_id");

            migrationBuilder.AddForeignKey(
                name: "fk_image_restoration_jobs_families_family_id",
                table: "image_restoration_jobs",
                column: "family_id",
                principalTable: "families",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_image_restoration_jobs_families_family_id",
                table: "image_restoration_jobs");

            migrationBuilder.DropIndex(
                name: "ix_image_restoration_jobs_family_id",
                table: "image_restoration_jobs");

            migrationBuilder.DropColumn(
                name: "family_id",
                table: "image_restoration_jobs");

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
