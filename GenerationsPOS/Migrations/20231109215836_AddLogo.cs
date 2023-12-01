using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenerationsPOS.Migrations
{
    /// <inheritdoc />
    public partial class AddLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoFileName",
                table: "Configuration",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoFileName",
                table: "Configuration");
        }
    }
}
