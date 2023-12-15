using GenerationsPOS.Data;
using GenerationsPOS.PointOfSale.Invoices;
using GenerationsPOS.QuickBooks;
using Interop.QBFC16;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenerationsPOS.PointOfSale.Accounting
{
    /// <summary>
    /// The 'QuickBooks' integration for this POS software
    /// Maintains a QuickBooks integration instance and maps the operations for this POS interface to more specific operations
    /// Maintains a cache of QuickBooks objects for this company 
    /// </summary>
    public class QuickBooksDesktop : IAccounting
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private QuickBooksConnection QuickBooks;

        private IEnumerable<Consignor> Consignors;
        private IEnumerable<string> ConsignorCodes;

        private IEnumerable<Customer> Customers;
        private IEnumerable<string> CustomerNames;

        public QuickBooksDesktop(CompanySettings config)
        {
            QuickBooks = new QuickBooksConnection(config);

            Consignors = new List<Consignor>();
            ConsignorCodes = new List<string>();

            Customers = new List<Customer>();
            CustomerNames = new List<string>();
        }

        public double TaxRate => QuickBooks.TaxRate;

        public void Connect()
        {
            QuickBooks.EstablishConnection();
            UpdateData();
        }

        public void Disconnect()
        {
            QuickBooks.Disconnect();
        }

        public bool IsConnected => QuickBooks.SessionActive;

        public void UpdateData()
        {
            UpdateConsignors();
            UpdateCustomers();
            QuickBooks.UpdateEntities();
        }

        public void UpdateConsignors()
        {
            Consignors = QuickBooks.GetAllVendors().Select(v => new Consignor()
            {
                QB = new QBObject
                {
                    Id = v.ListID.GetValue(),
                    Name = v.Name.GetValue(),
                }
            });
            ConsignorCodes = Consignors.Select(c => c.QB.Name).ToList();
        }

        public IEnumerable<Consignor> AllConsignors() => Consignors;

        public IEnumerable<string> AllConsignorCodes => ConsignorCodes;

        public IEnumerable<Consignor> FindConsignors(string startsWith) => Consignors.Where(c => c.QB.Name.StartsWith(startsWith,StringComparison.OrdinalIgnoreCase));

        public Consignor? GetConsignor(string name)
        {
            var match = Consignors.Where(c => c.QB.Name.Equals(name,StringComparison.OrdinalIgnoreCase));
            var count = match.Count();
            if (count > 0)
            {
                if(count > 1)
                {
                    Logger.Info($"Multiple consignors matched to name {name}");
                }
                return match.First();
            }
            else
            {
                return null;
            }
        }

        public void UpdateCustomers()
        {
            Customers = QuickBooks.GetAllCustomerJobs().Select(c => new Customer()
            {
                QB = new QBObject
                {
                    Id = c.ListID.GetValue(),
                    Name = c.Name.GetValue(),
                }
            });
            CustomerNames = Customers.Select(c => c.QB.Name).ToList();
        }

        public IEnumerable<Customer> AllCustomers() => Customers;

        public IEnumerable<string> AllCustomerNames => CustomerNames;

        public IEnumerable<Customer> FindCustomers(string startsWith) => Customers.Where(c => c.QB.Name.StartsWith(startsWith,StringComparison.OrdinalIgnoreCase));

        public Customer? GetCustomer(string name)
        {
            var match = Customers.Where(c => c.QB.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            var count = match.Count();
            if (count > 0)
            {
                if (count > 1)
                {
                    Logger.Info($"Multiple customers matched to name {name}");
                }
                return match.First();
            }
            else
            {
                return null;
            }
        }

        public void CommitInvoice(CustomerInvoice invoice)
        {
            // Create invoice
            IInvoiceRet? qbInvoice = null;
            try
            {
                qbInvoice = QuickBooks.CreateInvoice(invoice.CustomerJob, invoice.LineItems.LineItems, !invoice.OmitTax);
            }
            catch  (Exception ex)
            {
                throw new InvoiceCreationException($"An error occured sending this invoice to QuickBooks. Please check that QB is running and connected before retrying. Error: {ex.Message}", ex);
            }

            // Receive payments for invoice
            try
            {
                if(qbInvoice != null)
                {
                    QuickBooks.ReceivePayment(qbInvoice, invoice.Payments);
                }
            }
            catch (Exception ex)
            {
                throw new InvoicePaymentsException($"An error occured while sending the payment details to QuickBooks. The invoice itself seems to have been created, so you will want to check the state in QuickBooks and receive payments manually. Error: {ex.Message}", ex);
            }
        }
    }
}
