using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenerationsPOS.Migrations
{
    /// <inheritdoc />
    public partial class AddIncomeAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxName",
                table: "Configuration",
                newName: "IncomeAccount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IncomeAccount",
                table: "Configuration",
                newName: "TaxName");
        }
    }
}
