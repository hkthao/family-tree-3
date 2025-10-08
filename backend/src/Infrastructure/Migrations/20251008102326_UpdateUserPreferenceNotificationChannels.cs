using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserPreferenceNotificationChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NotificationsEnabled",
                table: "UserPreferences",
                newName: "SmsNotificationsEnabled");

            migrationBuilder.AddColumn<bool>(
                name: "EmailNotificationsEnabled",
                table: "UserPreferences",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "InAppNotificationsEnabled",
                table: "UserPreferences",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailNotificationsEnabled",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "InAppNotificationsEnabled",
                table: "UserPreferences");

            migrationBuilder.RenameColumn(
                name: "SmsNotificationsEnabled",
                table: "UserPreferences",
                newName: "NotificationsEnabled");
        }
    }
}
