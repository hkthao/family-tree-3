using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventLunarDateAndCalendarType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_year_estimated",
                table: "member_stories");

            migrationBuilder.DropColumn(
                name: "thumbnail_path",
                table: "family_media");

            migrationBuilder.DropColumn(
                name: "end_date",
                table: "events");

            migrationBuilder.DropColumn(
                name: "location",
                table: "events");

            migrationBuilder.RenameColumn(
                name: "start_date",
                table: "events",
                newName: "solar_date");

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

            migrationBuilder.AddColumn<int>(
                name: "calendar_type",
                table: "events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "day",
                table: "events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_leap_month",
                table: "events",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "month",
                table: "events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "repeat_rule",
                table: "events",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "calendar_type",
                table: "events");

            migrationBuilder.DropColumn(
                name: "day",
                table: "events");

            migrationBuilder.DropColumn(
                name: "is_leap_month",
                table: "events");

            migrationBuilder.DropColumn(
                name: "month",
                table: "events");

            migrationBuilder.DropColumn(
                name: "repeat_rule",
                table: "events");

            migrationBuilder.RenameColumn(
                name: "solar_date",
                table: "events",
                newName: "start_date");

            migrationBuilder.AddColumn<bool>(
                name: "is_year_estimated",
                table: "member_stories",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "thumbnail_path",
                table: "family_media",
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

            migrationBuilder.AddColumn<DateTime>(
                name: "end_date",
                table: "events",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location",
                table: "events",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
