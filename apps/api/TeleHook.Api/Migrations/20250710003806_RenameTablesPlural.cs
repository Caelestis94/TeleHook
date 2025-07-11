using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeleHook.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesPlural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Webhook_Bot_BotId",
                table: "Webhook");

            migrationBuilder.DropForeignKey(
                name: "FK_WebhookLogs_Webhook_WebhookId",
                table: "WebhookLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WebhookStat_Webhook_WebhookId",
                table: "WebhookStat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WebhookStat",
                table: "WebhookStat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Webhook",
                table: "Webhook");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bot",
                table: "Bot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppSetting",
                table: "AppSetting");

            migrationBuilder.RenameTable(
                name: "WebhookStat",
                newName: "WebhookStats");

            migrationBuilder.RenameTable(
                name: "Webhook",
                newName: "Webhooks");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Bot",
                newName: "Bots");

            migrationBuilder.RenameTable(
                name: "AppSetting",
                newName: "AppSettings");

            migrationBuilder.RenameIndex(
                name: "IX_Webhook_UUID",
                table: "Webhooks",
                newName: "IX_Webhooks_UUID");

            migrationBuilder.RenameIndex(
                name: "IX_User_Username",
                table: "Users",
                newName: "IX_Users_Username");

            migrationBuilder.RenameIndex(
                name: "IX_User_OidcId",
                table: "Users",
                newName: "IX_Users_OidcId");

            migrationBuilder.RenameIndex(
                name: "IX_User_Email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Bot_Name",
                table: "Bots",
                newName: "IX_Bots_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WebhookStats",
                table: "WebhookStats",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Webhooks",
                table: "Webhooks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bots",
                table: "Bots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppSettings",
                table: "AppSettings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WebhookLogs_Webhooks_WebhookId",
                table: "WebhookLogs",
                column: "WebhookId",
                principalTable: "Webhooks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WebhookStats_Webhooks_WebhookId",
                table: "WebhookStats",
                column: "WebhookId",
                principalTable: "Webhooks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Webhooks_Bots_BotId",
                table: "Webhooks",
                column: "BotId",
                principalTable: "Bots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebhookLogs_Webhooks_WebhookId",
                table: "WebhookLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WebhookStats_Webhooks_WebhookId",
                table: "WebhookStats");

            migrationBuilder.DropForeignKey(
                name: "FK_Webhooks_Bots_BotId",
                table: "Webhooks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Webhooks",
                table: "Webhooks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WebhookStats",
                table: "WebhookStats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bots",
                table: "Bots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppSettings",
                table: "AppSettings");

            migrationBuilder.RenameTable(
                name: "Webhooks",
                newName: "Webhook");

            migrationBuilder.RenameTable(
                name: "WebhookStats",
                newName: "WebhookStat");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "Bots",
                newName: "Bot");

            migrationBuilder.RenameTable(
                name: "AppSettings",
                newName: "AppSetting");

            migrationBuilder.RenameIndex(
                name: "IX_Webhooks_UUID",
                table: "Webhook",
                newName: "IX_Webhook_UUID");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Username",
                table: "User",
                newName: "IX_User_Username");

            migrationBuilder.RenameIndex(
                name: "IX_Users_OidcId",
                table: "User",
                newName: "IX_User_OidcId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "User",
                newName: "IX_User_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Bots_Name",
                table: "Bot",
                newName: "IX_Bot_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Webhook",
                table: "Webhook",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WebhookStat",
                table: "WebhookStat",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bot",
                table: "Bot",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppSetting",
                table: "AppSetting",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Webhook_Bot_BotId",
                table: "Webhook",
                column: "BotId",
                principalTable: "Bot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WebhookLogs_Webhook_WebhookId",
                table: "WebhookLogs",
                column: "WebhookId",
                principalTable: "Webhook",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WebhookStat_Webhook_WebhookId",
                table: "WebhookStat",
                column: "WebhookId",
                principalTable: "Webhook",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
