using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMemberFaceSchemaManually : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old BoundingBox columns


            // Add the new bounding_box JSON column
            migrationBuilder.AddColumn<string>(
                name: "bounding_box",
                table: "member_faces",
                type: "json", // Use JSON type for MySQL 8+, longtext for older versions or if unsure
                nullable: false, // BoundingBox is not nullable in the model
                defaultValue: "{}"); // Default empty JSON object

            // OPTIONAL: Data migration - if you had existing data in BoundingBox_X, Y, Width, Height
            // You would write SQL here to combine them into the new 'bounding_box' JSON column.
            // Example (MySQL):
            // migrationBuilder.Sql(@"
            //    UPDATE member_faces
            //    SET bounding_box = JSON_OBJECT(
            //        'X', BoundingBox_X,
            //        'Y', BoundingBox_Y,
            //        'Width', BoundingBox_Width,
            //        'Height', BoundingBox_Height
            //    )
            //    WHERE BoundingBox_X IS NOT NULL;
            // ");
            // This assumes BoundingBox_X etc. were nullable. If not, the WHERE clause is not needed.

            // The AlterColumn for family_dicts can remain or be removed, it's not directly related to MemberFace
            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "family_dicts",
                type: "char(36)",
                maxLength: 50,
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(string),
                oldType: "char(50)",
                oldMaxLength: 50)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the new bounding_box JSON column
            migrationBuilder.DropColumn(
                name: "bounding_box",
                table: "member_faces");

            // Re-add the old BoundingBox columns
            migrationBuilder.AddColumn<double>(
                name: "BoundingBox_X",
                table: "member_faces",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BoundingBox_Y",
                table: "member_faces",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BoundingBox_Width",
                table: "member_faces",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BoundingBox_Height",
                table: "member_faces",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            // The AlterColumn for family_dicts can remain or be removed
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
