using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.Data
{
    /// <summary>
    /// DatabaseContext establishes the local database access 
    /// For updates: 
    /// dotnet ef migrations add Name
    /// dotnet ef database update
    /// </summary>
    public class DatabaseContext : DbContext
    {
        private readonly string Path;

        public DbSet<CompanySettings> Configuration { get; set; }

        public DatabaseContext(string path)
        {
            Path = path;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={Path}");
    }

    /// <summary>
    /// Factory for the designer to create our DatabaseContext object (using a GenerationsPOS.db file in the local directory)
    /// </summary>
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args) => new("GenerationsPOS.db");
    }

    public class CompanySettings
    {
        public enum Theme : int
        {
            Light = 1,
            Dark = 2
        }

        [Key]
        public string Name { get; set; } = "Generations";
        public string CompanyName { get; set; } = "Generations";
        public string? LogoFileName { get; set; } = null;
        public string Header { get; set; } = "Receipt Header not Set";
        public string Footer { get; set; } = "ALL SALES FINAL";
        public string DefaultCustomerJobName { get; set; } = "MISC SALE";
        public string CardPaymentType { get; set; } = "CMBCARD";
        public string CashPaymentType { get; set; } = "CASH";
        public string CheckPaymentType { get; set; } = "CHECK";
        public string ConsignorCreditPaymentType { get; set; } = "CONS $";
        public string PurchaseAccount { get; set; } = "Purchases - Consignment";
        public string AssetAccount { get; set; } = "Inventory Asset";
        public string IncomeAccount { get; set; } = "Sales - Consignment";
        public int UserTheme { get; set; } = 1;
        public int COMPort { get; set; } = 8;

    }
}
