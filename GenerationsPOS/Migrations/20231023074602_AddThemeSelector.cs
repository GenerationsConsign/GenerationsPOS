using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenerationsPOS.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeSelector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserTheme",
                table: "Configuration",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserTheme",
                table: "Configuration");
        }
    }
}
