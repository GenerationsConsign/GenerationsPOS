using GenerationsPOS.Data;
using GenerationsPOS.PointOfSale.Accounting;
using GenerationsPOS.PointOfSale.Hardware;
using GenerationsPOS.PointOfSale.Hardware.Metrics;
using GenerationsPOS.PointOfSale.Invoices;
using GenerationsPOS.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.PointOfSale
{
    /// <summary>
    /// Interface for main GenerationsCore class which maintains state and access to some operations across the application
    /// This class should not interact with the UI, but the UI will call this manager to perform operations on hardware etc.
    /// This is to maintain separation of the UI and application logic 
    /// </summary>
    public interface IGenerationsCore
    {
        public IReceiptPrinter ReceiptPrinter { get; }

        public ICashDrawer CashDrawer {  get; }

        public DatabaseContext Database {  get; }

        public CompanySettings Configuration { get; }

        public UniversalCore Util { get; }

        public InvoiceManager Invoices { get; }
        
        public SystemStatus Status { get; }

        public IAccounting AccountingIntegration { get; }
    }
}
