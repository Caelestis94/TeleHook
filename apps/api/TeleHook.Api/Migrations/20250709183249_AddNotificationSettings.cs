using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeleHook.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableFailureNotifications",
                table: "AppSetting",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NotificationBotToken",
                table: "AppSetting",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotificationChatId",
                table: "AppSetting",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotificationTopicId",
                table: "AppSetting",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AppSetting",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "NotificationBotToken", "NotificationChatId", "NotificationTopicId" },
                values: new object[] { null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableFailureNotifications",
                table: "AppSetting");

            migrationBuilder.DropColumn(
                name: "NotificationBotToken",
                table: "AppSetting");

            migrationBuilder.DropColumn(
                name: "NotificationChatId",
                table: "AppSetting");

            migrationBuilder.DropColumn(
                name: "NotificationTopicId",
                table: "AppSetting");
        }
    }
}
