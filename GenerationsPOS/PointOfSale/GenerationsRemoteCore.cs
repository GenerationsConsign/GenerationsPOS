using GenerationsPOS.Data;
using GenerationsPOS.PointOfSale.Accounting;
using GenerationsPOS.PointOfSale.Hardware;
using GenerationsPOS.PointOfSale.Hardware.Metrics;
using GenerationsPOS.PointOfSale.Invoices;
using System;

namespace GenerationsPOS.PointOfSale
{
    /// <summary>
    /// GenerationsCore implementation for the desktop client
    /// </summary>
    public class GenerationsRemoteCore : IGenerationsCore
    {
        private readonly IReceiptPrinter _ReceiptPrinter;
        private readonly ICashDrawer _CashDrawer;
        private readonly UniversalCore _Util;
        private readonly InvoiceManager _Invoices;
        private readonly SystemStatus _Status;

        public GenerationsRemoteCore(UniversalCore util)
        {
            _ReceiptPrinter = new RemotePrinter();
            _CashDrawer = new RemoteDrawer();
            _Util = util;
            _Invoices = new(this);
            _Status = new();
        }

        public IReceiptPrinter ReceiptPrinter => _ReceiptPrinter;

        public ICashDrawer CashDrawer => _CashDrawer;

        public DatabaseContext Database => throw new NotImplementedException();

        public CompanySettings Configuration => throw new NotImplementedException();

        public UniversalCore Util => _Util;

        public InvoiceManager Invoices => _Invoices;

        public SystemStatus Status => _Status;

        public IAccounting AccountingIntegration => throw new NotImplementedException();
    }
}
