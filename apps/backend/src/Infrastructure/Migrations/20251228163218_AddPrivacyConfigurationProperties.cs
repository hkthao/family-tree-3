using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivacyConfigurationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_memory_items_families_family_id",
                table: "memory_items");

            migrationBuilder.AddColumn<string>(
                name: "public_event_properties",
                table: "privacy_configurations",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "public_family_location_properties",
                table: "privacy_configurations",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "public_family_properties",
                table: "privacy_configurations",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "public_found_face_properties",
                table: "privacy_configurations",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "public_member_face_properties",
                table: "privacy_configurations",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "public_memory_item_properties",
                table: "privacy_configurations",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.AddForeignKey(
                name: "fk_memory_items_families_family_id",
                table: "memory_items",
                column: "family_id",
                principalTable: "families",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_memory_items_families_family_id",
                table: "memory_items");

            migrationBuilder.DropColumn(
                name: "public_event_properties",
                table: "privacy_configurations");

            migrationBuilder.DropColumn(
                name: "public_family_location_properties",
                table: "privacy_configurations");

            migrationBuilder.DropColumn(
                name: "public_family_properties",
                table: "privacy_configurations");

            migrationBuilder.DropColumn(
                name: "public_found_face_properties",
                table: "privacy_configurations");

            migrationBuilder.DropColumn(
                name: "public_member_face_properties",
                table: "privacy_configurations");

            migrationBuilder.DropColumn(
                name: "public_memory_item_properties",
                table: "privacy_configurations");

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

            migrationBuilder.AddForeignKey(
                name: "fk_memory_items_families_family_id",
                table: "memory_items",
                column: "family_id",
                principalTable: "families",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
