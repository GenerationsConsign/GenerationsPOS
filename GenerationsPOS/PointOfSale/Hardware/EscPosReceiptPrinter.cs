using ESCPOS_NET;
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
    internal class EscPosReceiptPrinter : IReceiptPrinter
    {
        private readonly EpsonReceiptPrinter Printer;

        public EscPosReceiptPrinter(EpsonReceiptPrinter printer)
        {
            Printer = printer;
        }

        public bool IsConnected => Printer.Connected;

        public void Reconnect() => Printer.Reconnect();

        public void Print(CompanySettings config, CustomerInvoice invoice) => Printer.Print(config, invoice);

        public void TestPrint(CompanySettings config) => Printer.TestPrint(config);
    }
}
