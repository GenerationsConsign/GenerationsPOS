using GenerationsPOS.Data;
using GenerationsPOS.PointOfSale.Invoices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.PointOfSale.Hardware
{
    /// <summary>
    /// Our current implementation for cash drawer operation - kick signal is sent to an attached ESC/POS receipt printer 
    /// The printer in turn sends the appropriate 24v signal to the cash drawer
    /// </summary>
    internal class RemotePrinter : IReceiptPrinter
    {
        public bool IsConnected => false;

        public void Reconnect() => throw new NotImplementedException();

        public void Print(CompanySettings config, CustomerInvoice? invoice) => throw new NotImplementedException();

        public void TestPrint(CompanySettings config) => throw new NotImplementedException();
    }
}
