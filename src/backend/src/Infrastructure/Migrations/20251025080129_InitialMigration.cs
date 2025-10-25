using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "families",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    _name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _address = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _avatar_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _visibility = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _total_members = table.Column<int>(type: "int", nullable: false),
                    _total_generations = table.Column<int>(type: "int", nullable: false),
                    _created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _created_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    _last_modified_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_families", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "file_metadata",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    file_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    storage_provider = table.Column<int>(type: "int", nullable: false),
                    content_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    used_by_entity = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _used_by_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    _created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _created_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    _last_modified_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file_metadata", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notification_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    event_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    channel = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    subject = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    body = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    format = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    language_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    placeholders = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_modified_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    last_modified_by = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_templates", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NotificationType = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientUserId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SenderUserId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReadAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    FamilyId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    _name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _start_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    _end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    _location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    family_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    _type = table.Column<int>(type: "int", nullable: false),
                    _color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _created_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    _last_modified_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_events_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    _last_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _first_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _nickname = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _gender = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _date_of_birth = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    _date_of_death = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    _place_of_birth = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _place_of_death = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _occupation = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _avatar_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _biography = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    family_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    _is_root = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    _created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _created_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    _last_modified_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_members_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EventMember",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RelatedMembersId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventMember", x => new { x.EventId, x.RelatedMembersId });
                    table.ForeignKey(
                        name: "FK_EventMember_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventMember_members_RelatedMembersId",
                        column: x => x.RelatedMembersId,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "relationships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    source_member_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    target_member_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    _type = table.Column<int>(type: "int", nullable: false),
                    _order = table.Column<int>(type: "int", nullable: true),
                    _family_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    _created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _created_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    _last_modified_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relationships", x => x.id);
                    table.ForeignKey(
                        name: "FK_relationships_families__family_id",
                        column: x => x._family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_relationships_members_source_member_id",
                        column: x => x.source_member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_relationships_members_target_member_id",
                        column: x => x.target_member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "family_users",
                columns: table => new
                {
                    family_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_profile_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    _role = table.Column<int>(type: "int", nullable: false),
                    _id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family_users", x => new { x.family_id, x.user_profile_id });
                    table.ForeignKey(
                        name: "FK_family_users_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_preferences",
                columns: table => new
                {
                    user_profile_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    _theme = table.Column<int>(type: "int", nullable: false),
                    _language = table.Column<int>(type: "int", nullable: false),
                    _email_notifications_enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    _sms_notifications_enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    _in_app_notifications_enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    _id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    _created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _created_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    _last_modified_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_preferences", x => x.user_profile_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    external_id = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _avatar = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _user_preference_user_profile_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    _created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _created_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    _last_modified_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_profiles_user_preferences__user_preference_user_profile~",
                        column: x => x._user_preference_user_profile_id,
                        principalTable: "user_preferences",
                        principalColumn: "user_profile_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GroupId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ActivitySummary = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivities_user_profiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EventMember_RelatedMembersId",
                table: "EventMember",
                column: "RelatedMembersId");

            migrationBuilder.CreateIndex(
                name: "IX_events_code",
                table: "events",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_events_family_id",
                table: "events",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "IX_families_code",
                table: "families",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_family_users_user_profile_id",
                table: "family_users",
                column: "user_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_members_code",
                table: "members",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_members_family_id",
                table: "members",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationships__family_id",
                table: "relationships",
                column: "_family_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationships_source_member_id",
                table: "relationships",
                column: "source_member_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationships_target_member_id",
                table: "relationships",
                column: "target_member_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_profiles__user_preference_user_profile_id",
                table: "user_profiles",
                column: "_user_preference_user_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_profiles_external_id",
                table: "user_profiles",
                column: "external_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_UserProfileId",
                table: "UserActivities",
                column: "UserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_family_users_user_profiles_user_profile_id",
                table: "family_users",
                column: "user_profile_id",
                principalTable: "user_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_preferences_user_profiles_user_profile_id",
                table: "user_preferences",
                column: "user_profile_id",
                principalTable: "user_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_preferences_user_profiles_user_profile_id",
                table: "user_preferences");

            migrationBuilder.DropTable(
                name: "EventMember");

            migrationBuilder.DropTable(
                name: "family_users");

            migrationBuilder.DropTable(
                name: "file_metadata");

            migrationBuilder.DropTable(
                name: "notification_templates");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "relationships");

            migrationBuilder.DropTable(
                name: "UserActivities");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropTable(
                name: "families");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "user_preferences");
        }
    }
}
