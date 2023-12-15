using GenerationsPOS.PointOfSale.Invoices;
using NLog;
using System.Collections.Generic;

namespace GenerationsPOS.PointOfSale.Accounting
{
    public class LocalAccounting : IAccounting
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public bool IsConnected => false;

        public IEnumerable<string> AllConsignorCodes => new List<string>();

        public IEnumerable<string> AllCustomerNames => new List<string>();

        public double TaxRate => 5.5;

        public IEnumerable<Consignor> AllConsignors() => new List<Consignor>();

        public IEnumerable<Customer> AllCustomers() => new List<Customer>();

        public void CommitInvoice(CustomerInvoice invoice) => logger.Info("#CommitInvoice called");

        public void Connect() => logger.Info("#Connect called");

        public void Disconnect() => logger.Info("#Disconnect called");

        public IEnumerable<Consignor> FindConsignors(string startsWith) => new List<Consignor>();

        public Consignor? GetConsignor(string name) => null;

        public Customer? GetCustomer(string name) => null;

        public void UpdateData() => logger.Info("#UpdateInvoice called");
    }
}
