using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeleHook.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreateSettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LogLevel = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Warning"),
                    LogRetentionDays = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 7),
                    EnableWebhookLogging = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    WebhookLogRetentionDays = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    StatsDaysInterval = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 30),
                    AdditionalSettings = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now', 'utc')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now', 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSetting", x => x.Id);
                    table.CheckConstraint("CK_AppSettings_SingleRow", "Id = 1");
                });

            migrationBuilder.InsertData(
                table: "AppSetting",
                columns: new[] { "Id", "AdditionalSettings", "EnableWebhookLogging", "LogLevel", "LogRetentionDays", "StatsDaysInterval" },
                values: new object[] { 1, null, true, "Warning", 7, 30 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSetting");
        }
    }
}
