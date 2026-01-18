using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanInitialSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "locations",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
            //         name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         latitude = table.Column<double>(type: "double", nullable: true),
            //         longitude = table.Column<double>(type: "double", nullable: true),
            //         address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         location_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         accuracy = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         source = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //         deleted_by = table.Column<string>(type: "longtext", nullable: true)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         deleted_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //         created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //         created_by = table.Column<string>(type: "longtext", nullable: true)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //         last_modified_by = table.Column<string>(type: "longtext", nullable: true)
            //             .Annotation("MySql:CharSet", "utf8mb4")
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_locations", x => x.id);
            //     })
            //     .Annotation("MySql:CharSet", "utf8mb4");

            // migrationBuilder.CreateTable(
            //     name: "family_locations",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
            //         family_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
            //         location_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
            //         is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //         deleted_by = table.Column<string>(type: "longtext", nullable: true)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         deleted_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //         created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //         created_by = table.Column<string>(type: "longtext", nullable: true)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //         last_modified_by = table.Column<string>(type: "longtext", nullable: true)
            //             .Annotation("MySql:CharSet", "utf8mb4")
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_family_locations", x => x.id);
            //     })
            //     .Annotation("MySql:CharSet", "utf8mb4");

            // migrationBuilder.CreateIndex(
            //     name: "ix_family_locations_location_id",
            //     table: "family_locations",
            //     column: "location_id");

            // migrationBuilder.AddForeignKey(
            //     name: "fk_family_locations_locations_location_id",
            //     table: "family_locations",
            //     column: "location_id",
            //     principalTable: "locations",
            //     principalColumn: "id",
            //     onDelete: ReferentialAction.Restrict);

            // migrationBuilder.AddForeignKey(
            //     name: "fk_location_links_locations_location_id",
            //     table: "location_links",
            //     column: "location_id",
            //     principalTable: "locations",
            //     principalColumn: "id",
            //     onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_family_locations_locations_location_id",
                table: "family_locations");

            migrationBuilder.DropForeignKey(
                name: "fk_location_links_locations_location_id",
                table: "location_links");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropIndex(
                name: "ix_family_locations_location_id",
                table: "family_locations");

            migrationBuilder.DropColumn(
                name: "link_type",
                table: "location_links");

            migrationBuilder.DropColumn(
                name: "location_id",
                table: "family_locations");

            migrationBuilder.RenameColumn(
                name: "location_id",
                table: "location_links",
                newName: "family_location_id");

            migrationBuilder.RenameIndex(
                name: "ix_location_links_location_id",
                table: "location_links",
                newName: "ix_location_links_family_location_id");

            migrationBuilder.AddColumn<string>(
                name: "accuracy",
                table: "family_locations",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "family_locations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "family_locations",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "family_locations",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location_type",
                table: "family_locations",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "family_locations",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "family_locations",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "family_locations",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
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

            migrationBuilder.AddForeignKey(
                name: "fk_location_links_family_locations_family_location_id",
                table: "location_links",
                column: "family_location_id",
                principalTable: "family_locations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
