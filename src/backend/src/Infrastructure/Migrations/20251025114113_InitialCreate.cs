using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActivities_user_profiles_UserProfileId",
                table: "UserActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserActivities",
                table: "UserActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationPreferences",
                table: "NotificationPreferences");

            migrationBuilder.RenameTable(
                name: "UserActivities",
                newName: "user_activity");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "notification");

            migrationBuilder.RenameTable(
                name: "NotificationPreferences",
                newName: "notification_preference");

            migrationBuilder.RenameColumn(
                name: "UserProfileId",
                table: "user_activity",
                newName: "user_profile_id");

            migrationBuilder.RenameColumn(
                name: "TargetType",
                table: "user_activity",
                newName: "target_type");

            migrationBuilder.RenameColumn(
                name: "TargetId",
                table: "user_activity",
                newName: "target_id");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "user_activity",
                newName: "group_id");

            migrationBuilder.RenameColumn(
                name: "ActivitySummary",
                table: "user_activity",
                newName: "activity_summary");

            migrationBuilder.RenameColumn(
                name: "ActionType",
                table: "user_activity",
                newName: "action_type");

            migrationBuilder.RenameIndex(
                name: "IX_UserActivities_UserProfileId",
                table: "user_activity",
                newName: "IX_user_activity_user_profile_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "notification",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "notification",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "notification",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "notification",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "SenderUserId",
                table: "notification",
                newName: "sender_user_id");

            migrationBuilder.RenameColumn(
                name: "RecipientUserId",
                table: "notification",
                newName: "recipient_user_id");

            migrationBuilder.RenameColumn(
                name: "ReadAt",
                table: "notification",
                newName: "read_at");

            migrationBuilder.RenameColumn(
                name: "FamilyId",
                table: "notification",
                newName: "family_id");

            migrationBuilder.RenameColumn(
                name: "Enabled",
                table: "notification_preference",
                newName: "enabled");

            migrationBuilder.RenameColumn(
                name: "Channel",
                table: "notification_preference",
                newName: "channel");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "notification_preference",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "NotificationType",
                table: "notification_preference",
                newName: "notification_type");

            migrationBuilder.AlterColumn<string>(
                name: "target_id",
                table: "user_activity",
                type: "varchar(36)",
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "activity_summary",
                table: "user_activity",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "metadata",
                table: "user_activity",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "notification",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "message",
                table: "notification",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "sender_user_id",
                table: "notification",
                type: "varchar(36)",
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "user_id",
                table: "notification_preference",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "notification_type",
                table: "notification_preference",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_activity",
                table: "user_activity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notification",
                table: "notification",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notification_preference",
                table: "notification_preference",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "text_chunk",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    family_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    metadata = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    embedding = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    score = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_text_chunk", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_notification_preference_user_id_notification_type_channel",
                table: "notification_preference",
                columns: new[] { "user_id", "notification_type", "channel" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_user_activity_user_profiles_user_profile_id",
                table: "user_activity",
                column: "user_profile_id",
                principalTable: "user_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_activity_user_profiles_user_profile_id",
                table: "user_activity");

            migrationBuilder.DropTable(
                name: "text_chunk");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_activity",
                table: "user_activity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notification_preference",
                table: "notification_preference");

            migrationBuilder.DropIndex(
                name: "ix_notification_preference_user_id_notification_type_channel",
                table: "notification_preference");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notification",
                table: "notification");

            migrationBuilder.DropColumn(
                name: "metadata",
                table: "user_activity");

            migrationBuilder.RenameTable(
                name: "user_activity",
                newName: "UserActivities");

            migrationBuilder.RenameTable(
                name: "notification_preference",
                newName: "NotificationPreferences");

            migrationBuilder.RenameTable(
                name: "notification",
                newName: "Notifications");

            migrationBuilder.RenameColumn(
                name: "user_profile_id",
                table: "UserActivities",
                newName: "UserProfileId");

            migrationBuilder.RenameColumn(
                name: "target_type",
                table: "UserActivities",
                newName: "TargetType");

            migrationBuilder.RenameColumn(
                name: "target_id",
                table: "UserActivities",
                newName: "TargetId");

            migrationBuilder.RenameColumn(
                name: "group_id",
                table: "UserActivities",
                newName: "GroupId");

            migrationBuilder.RenameColumn(
                name: "activity_summary",
                table: "UserActivities",
                newName: "ActivitySummary");

            migrationBuilder.RenameColumn(
                name: "action_type",
                table: "UserActivities",
                newName: "ActionType");

            migrationBuilder.RenameIndex(
                name: "IX_user_activity_user_profile_id",
                table: "UserActivities",
                newName: "IX_UserActivities_UserProfileId");

            migrationBuilder.RenameColumn(
                name: "enabled",
                table: "NotificationPreferences",
                newName: "Enabled");

            migrationBuilder.RenameColumn(
                name: "channel",
                table: "NotificationPreferences",
                newName: "Channel");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "NotificationPreferences",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "notification_type",
                table: "NotificationPreferences",
                newName: "NotificationType");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "Notifications",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Notifications",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Notifications",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "Notifications",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "sender_user_id",
                table: "Notifications",
                newName: "SenderUserId");

            migrationBuilder.RenameColumn(
                name: "recipient_user_id",
                table: "Notifications",
                newName: "RecipientUserId");

            migrationBuilder.RenameColumn(
                name: "read_at",
                table: "Notifications",
                newName: "ReadAt");

            migrationBuilder.RenameColumn(
                name: "family_id",
                table: "Notifications",
                newName: "FamilyId");

            migrationBuilder.AlterColumn<string>(
                name: "TargetId",
                table: "UserActivities",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldMaxLength: 36,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ActivitySummary",
                table: "UserActivities",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "NotificationPreferences",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "NotificationType",
                table: "NotificationPreferences",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "SenderUserId",
                table: "Notifications",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldMaxLength: 36,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserActivities",
                table: "UserActivities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationPreferences",
                table: "NotificationPreferences",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivities_user_profiles_UserProfileId",
                table: "UserActivities",
                column: "UserProfileId",
                principalTable: "user_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
