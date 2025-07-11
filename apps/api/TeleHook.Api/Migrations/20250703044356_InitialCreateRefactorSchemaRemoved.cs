using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeleHook.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateRefactorSchemaRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    BotToken = table.Column<string>(type: "TEXT", nullable: false),
                    ChatId = table.Column<string>(type: "TEXT", nullable: false),
                    HasPassedTest = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bot", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "admin"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OidcId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    AuthProvider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, defaultValue: "credentials")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.CheckConstraint("CK_User_AuthMethod", "(PasswordHash IS NOT NULL AND AuthProvider = 'credentials') OR (OidcId IS NOT NULL AND AuthProvider = 'oidc')");
                    table.CheckConstraint("CK_User_AuthProvider", "AuthProvider IN ('credentials', 'oidc')");
                    table.CheckConstraint("CK_User_Role", "Role IN ('admin', 'user')");
                });

            migrationBuilder.CreateTable(
                name: "Webhook",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    UUID = table.Column<string>(type: "TEXT", nullable: false),
                    BotId = table.Column<int>(type: "INTEGER", nullable: false),
                    TopicId = table.Column<string>(type: "TEXT", nullable: true),
                    MessageTemplate = table.Column<string>(type: "TEXT", nullable: false),
                    ParseMode = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "MarkdownV2"),
                    DisableWebPagePreview = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    DisableNotification = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsDisabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    IsProtected = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    SecretKey = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PayloadSample = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Webhook", x => x.Id);
                    table.CheckConstraint("CK_Webhook_ParseMode", "ParseMode IN ('HTML', 'Markdown', 'MarkdownV2')");
                    table.ForeignKey(
                        name: "FK_Webhook_Bot_BotId",
                        column: x => x.BotId,
                        principalTable: "Bot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WebhookLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WebhookId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestId = table.Column<string>(type: "TEXT", nullable: false),
                    HttpMethod = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "POST"),
                    RequestUrl = table.Column<string>(type: "TEXT", nullable: false),
                    RequestHeaders = table.Column<string>(type: "TEXT", nullable: true),
                    RequestBody = table.Column<string>(type: "TEXT", nullable: true),
                    ResponseStatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponseBody = table.Column<string>(type: "TEXT", nullable: true),
                    ProcessingTimeMs = table.Column<int>(type: "INTEGER", nullable: false),
                    PayloadValidated = table.Column<bool>(type: "INTEGER", nullable: false),
                    ValidationErrors = table.Column<string>(type: "TEXT", nullable: true),
                    MessageFormatted = table.Column<string>(type: "TEXT", nullable: true),
                    TelegramSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    TelegramResponse = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookLogs_Webhook_WebhookId",
                        column: x => x.WebhookId,
                        principalTable: "Webhook",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebhookStat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WebhookId = table.Column<int>(type: "INTEGER", nullable: true),
                    TotalRequests = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    SuccessfulRequests = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    FailedRequests = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ValidationFailures = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    TelegramFailures = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    TotalProcessingTimeMs = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L),
                    AvgProcessingTimeMs = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    MinProcessingTimeMs = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    MaxProcessingTimeMs = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookStat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookStat_Webhook_WebhookId",
                        column: x => x.WebhookId,
                        principalTable: "Webhook",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bot_Name",
                table: "Bot",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_OidcId",
                table: "User",
                column: "OidcId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Webhook_BotId",
                table: "Webhook",
                column: "BotId");

            migrationBuilder.CreateIndex(
                name: "IX_Webhook_IsDisabled",
                table: "Webhook",
                column: "IsDisabled");

            migrationBuilder.CreateIndex(
                name: "IX_Webhook_IsProtected",
                table: "Webhook",
                column: "IsProtected");

            migrationBuilder.CreateIndex(
                name: "IX_Webhook_UUID",
                table: "Webhook",
                column: "UUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookLog_CreatedAt",
                table: "WebhookLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookLog_RequestId",
                table: "WebhookLogs",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookLog_ResponseStatusCode",
                table: "WebhookLogs",
                column: "ResponseStatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookLog_WebhookId",
                table: "WebhookLogs",
                column: "WebhookId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookStat_Date",
                table: "WebhookStat",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookStat_Date_Id",
                table: "WebhookStat",
                columns: new[] { "Date", "WebhookId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookStat_WebhookId",
                table: "WebhookStat",
                column: "WebhookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "WebhookLogs");

            migrationBuilder.DropTable(
                name: "WebhookStat");

            migrationBuilder.DropTable(
                name: "Webhook");

            migrationBuilder.DropTable(
                name: "Bot");
        }
    }
}
