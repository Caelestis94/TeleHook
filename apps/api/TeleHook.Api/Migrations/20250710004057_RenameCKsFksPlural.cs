using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeleHook.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameCKsFksPlural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Webhook_ParseMode",
                table: "Webhooks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_User_AuthMethod",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_User_AuthProvider",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_User_Role",
                table: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_Webhook_IsProtected",
                table: "Webhooks",
                newName: "IX_Webhooks_IsProtected");

            migrationBuilder.RenameIndex(
                name: "IX_Webhook_IsDisabled",
                table: "Webhooks",
                newName: "IX_Webhooks_IsDisabled");

            migrationBuilder.RenameIndex(
                name: "IX_Webhook_BotId",
                table: "Webhooks",
                newName: "IX_Webhooks_BotId");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookStat_WebhookId",
                table: "WebhookStats",
                newName: "IX_WebhookStats_WebhookId");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookStat_Date_Id",
                table: "WebhookStats",
                newName: "IX_WebhookStats_Date_Id");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookStat_Date",
                table: "WebhookStats",
                newName: "IX_WebhookStats_Date");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookLog_WebhookId",
                table: "WebhookLogs",
                newName: "IX_WebhookLogs_WebhookId");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookLog_ResponseStatusCode",
                table: "WebhookLogs",
                newName: "IX_WebhookLogs_ResponseStatusCode");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookLog_RequestId",
                table: "WebhookLogs",
                newName: "IX_WebhookLogs_RequestId");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookLog_CreatedAt",
                table: "WebhookLogs",
                newName: "IX_WebhookLogs_CreatedAt");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Webhooks_ParseMode",
                table: "Webhooks",
                sql: "ParseMode IN ('HTML', 'Markdown', 'MarkdownV2')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_AuthMethod",
                table: "Users",
                sql: "(PasswordHash IS NOT NULL AND AuthProvider = 'credentials') OR (OidcId IS NOT NULL AND AuthProvider = 'oidc')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_AuthProvider",
                table: "Users",
                sql: "AuthProvider IN ('credentials', 'oidc')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Role",
                table: "Users",
                sql: "Role IN ('admin', 'user')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Webhooks_ParseMode",
                table: "Webhooks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_AuthMethod",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_AuthProvider",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Role",
                table: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_Webhooks_IsProtected",
                table: "Webhooks",
                newName: "IX_Webhook_IsProtected");

            migrationBuilder.RenameIndex(
                name: "IX_Webhooks_IsDisabled",
                table: "Webhooks",
                newName: "IX_Webhook_IsDisabled");

            migrationBuilder.RenameIndex(
                name: "IX_Webhooks_BotId",
                table: "Webhooks",
                newName: "IX_Webhook_BotId");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookStats_WebhookId",
                table: "WebhookStats",
                newName: "IX_WebhookStat_WebhookId");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookStats_Date_Id",
                table: "WebhookStats",
                newName: "IX_WebhookStat_Date_Id");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookStats_Date",
                table: "WebhookStats",
                newName: "IX_WebhookStat_Date");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookLogs_WebhookId",
                table: "WebhookLogs",
                newName: "IX_WebhookLog_WebhookId");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookLogs_ResponseStatusCode",
                table: "WebhookLogs",
                newName: "IX_WebhookLog_ResponseStatusCode");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookLogs_RequestId",
                table: "WebhookLogs",
                newName: "IX_WebhookLog_RequestId");

            migrationBuilder.RenameIndex(
                name: "IX_WebhookLogs_CreatedAt",
                table: "WebhookLogs",
                newName: "IX_WebhookLog_CreatedAt");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Webhook_ParseMode",
                table: "Webhooks",
                sql: "ParseMode IN ('HTML', 'Markdown', 'MarkdownV2')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_User_AuthMethod",
                table: "Users",
                sql: "(PasswordHash IS NOT NULL AND AuthProvider = 'credentials') OR (OidcId IS NOT NULL AND AuthProvider = 'oidc')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_User_AuthProvider",
                table: "Users",
                sql: "AuthProvider IN ('credentials', 'oidc')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_User_Role",
                table: "Users",
                sql: "Role IN ('admin', 'user')");
        }
    }
}
