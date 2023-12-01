using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenerationsPOS.Migrations
{
    /// <inheritdoc />
    public partial class InitalCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configuration",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Header = table.Column<string>(type: "TEXT", nullable: false),
                    Footer = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultCustomerJobName = table.Column<string>(type: "TEXT", nullable: false),
                    TaxName = table.Column<string>(type: "TEXT", nullable: false),
                    CardPaymentType = table.Column<string>(type: "TEXT", nullable: false),
                    CashPaymentType = table.Column<string>(type: "TEXT", nullable: false),
                    CheckPaymentType = table.Column<string>(type: "TEXT", nullable: false),
                    ConsignorCreditPaymentType = table.Column<string>(type: "TEXT", nullable: false),
                    COMPort = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuration", x => x.Name);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuration");
        }
    }
}
