using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeleHook.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLogPathToSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogPath",
                table: "AppSetting",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "/app/logs/telehook-.log");

            migrationBuilder.UpdateData(
                table: "AppSetting",
                keyColumn: "Id",
                keyValue: 1,
                column: "LogPath",
                value: "/app/logs/telehook-.log");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogPath",
                table: "AppSetting");
        }
    }
}
