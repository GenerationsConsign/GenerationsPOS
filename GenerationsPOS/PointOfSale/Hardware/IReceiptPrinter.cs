using GenerationsPOS.Data;
using GenerationsPOS.PointOfSale.Invoices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
