using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeleHook.Api.Migrations
{
    /// <inheritdoc />
    public partial class SavePendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UUID",
                table: "Webhooks",
                newName: "Uuid");

            migrationBuilder.RenameIndex(
                name: "IX_Webhooks_UUID",
                table: "Webhooks",
                newName: "IX_Webhooks_Uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Uuid",
                table: "Webhooks",
                newName: "UUID");

            migrationBuilder.RenameIndex(
                name: "IX_Webhooks_Uuid",
                table: "Webhooks",
                newName: "IX_Webhooks_UUID");
        }
    }
}
