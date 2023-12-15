using GenerationsPOS.PointOfSale.Invoices;
using System.Collections.Generic;

namespace GenerationsPOS.PointOfSale.Accounting
{
    /// <summary>
    /// Interface for 'accounting' methods as used for sales invoices
    /// Implementation may be changed if the company changes accounting software in the future
    /// </summary>
    public interface IAccounting
    {
        public void Connect();

        public void Disconnect();

        public bool IsConnected { get; }

        public void UpdateData();

        public IEnumerable<Consignor> AllConsignors();

        public IEnumerable<string> AllConsignorCodes { get; }

        public IEnumerable<Consignor> FindConsignors(string startsWith);

        public Consignor? GetConsignor(string name);

        public IEnumerable<Customer> AllCustomers();

        public IEnumerable<string> AllCustomerNames { get; }

        public Customer? GetCustomer(string name);

        public void CommitInvoice(CustomerInvoice invoice);

        public double TaxRate { get; }
    }
}
