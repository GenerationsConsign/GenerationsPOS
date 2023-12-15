using GenerationsPOS.Data;
using GenerationsPOS.PointOfSale.Invoices;

namespace GenerationsPOS.PointOfSale.Hardware
{
    public interface IReceiptPrinter
    {
        public bool IsConnected { get; }

        public void Reconnect();

        public void Print(CompanySettings config, CustomerInvoice invoice);

        public void TestPrint(CompanySettings config);
    }
}
