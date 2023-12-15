using System.Collections.Generic;

namespace GenerationsPOS.PointOfSale.Invoices
{
    /// <summary>
    /// Container for the invoices loaded into memory for editing
    /// </summary>
    public class InvoiceManager
    {
        private IGenerationsCore Generations;
        private uint TicketNumber = 1;

        public InvoiceManager(IGenerationsCore core)
        {
            Generations = core;
            ActiveInvoices = new();
            // Insert initial invoice
            CreateNewInvoice();
        }

        /// <summary>
        /// The "active" invoices - should be displayed to store POS 
        /// </summary>
        public List<CustomerInvoice> ActiveInvoices { get; }

        public CustomerInvoice CreateNewInvoice()
        {
            var invoice = new CustomerInvoice(Generations, TicketNumber++);
            ActiveInvoices.Add(invoice);
            return invoice;
        }

        public void DropInvoice(CustomerInvoice invoice) => ActiveInvoices.Remove(invoice);
    }
}
