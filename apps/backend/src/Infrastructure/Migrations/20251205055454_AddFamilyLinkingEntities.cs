using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyLinkingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "family_link_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    requesting_family_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    target_family_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    status = table.Column<int>(type: "int", nullable: false),
                    request_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    response_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("pk_family_link_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_family_link_requests_families_requesting_family_id",
                        column: x => x.requesting_family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_family_link_requests_families_target_family_id",
                        column: x => x.target_family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "family_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    family1_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    family2_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    link_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
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
                    table.PrimaryKey("pk_family_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_family_links_families_family1_id",
                        column: x => x.family1_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_family_links_families_family2_id",
                        column: x => x.family2_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_family_link_requests_requesting_family_id",
                table: "family_link_requests",
                column: "requesting_family_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_link_requests_target_family_id",
                table: "family_link_requests",
                column: "target_family_id");

            migrationBuilder.CreateIndex(
                name: "IX_family_links_family1_id_family2_id",
                table: "family_links",
                columns: ["family1_id", "family2_id"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_family_links_family2_id",
                table: "family_links",
                column: "family2_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "family_link_requests");

            migrationBuilder.DropTable(
                name: "family_links");

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
