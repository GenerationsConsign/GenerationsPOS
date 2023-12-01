using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenerationsPOS.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssetAccount",
                table: "Configuration",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PurchaseAccount",
                table: "Configuration",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetAccount",
                table: "Configuration");

            migrationBuilder.DropColumn(
                name: "PurchaseAccount",
                table: "Configuration");
        }
    }
}
