using CommunityToolkit.Mvvm.ComponentModel;
using GenerationsPOS.PointOfSale.Accounting;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.PointOfSale.Invoices
{
    public class CustomerInvoice : ObservableObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IGenerationsCore Generations;
        private uint InvoiceID;
        private string _Name;
        private string? _Customer;
        private bool _OmitTax;

        /// <summary>
        /// Creates a "new" customer invoice (ready for entry, with one line item to start)
        /// Not used for invoice "import"
        /// </summary>
        public CustomerInvoice(IGenerationsCore core, uint id)
        {
            Generations = core;
            InvoiceID = id;
            // Initial invoice name: Invoice #1
            _Name = $"Invoice #{id}";
            _Customer = null;
            _OmitTax = false;

            LineItems = new InvoiceItems();
            Payments = new InvoicePayments();

            LineItems.AddLineItemsUpdateHandler(PriceUpdating);
            Payments.PaymentsUpdate += PaymentsUpdating;
        }

        private void PriceUpdating()
        {
            UpdateInvoiceName();
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(TaxAmount));
            OnPropertyChanged(nameof(InvoiceTotal));
            OnPropertyChanged(nameof(PaymentsRequired));
            OnPropertyChanged(nameof(InvoiceUnpaid));
        }

        private void PaymentsUpdating()
        {
            OnPropertyChanged(nameof(PaymentsReceived));
            OnPropertyChanged(nameof(PaymentsRequired));
        }

        public decimal TaxRate
        {
            get => (decimal)Generations.AccountingIntegration.TaxRate / 100;
        }

        /// <summary>
        /// The current UI-visible name for this invoice. Use customer name if specified on invoice
        /// </summary>
        public string InvoiceName
        {
            get
            {
                var total = string.Format("{0:c}", InvoiceTotal);
                return $"{_Name} ({total})";
            }
            set
            {
                _Name = value;
                OnPropertyChanged();
            }
        }

        private void UpdateInvoiceName()
        {
            InvoiceName = Customer ?? $"Invoice #{InvoiceID}";
        }

        /// <summary>
        /// Access to the line items for this invoice
        /// </summary>
        public InvoiceItems LineItems { get; set; }

        /// <summary>
        /// Access to payment details for this invoice
        /// </summary>
        public InvoicePayments Payments { get; set; }

        /// <summary>
        /// The sum of all item prices on this invoice (pre-tax)
        /// </summary>
        public decimal Subtotal => LineItems.ItemTotal;

        /// <summary>
        /// True if this invoice should be tax-exempted
        /// </summary>
        public bool OmitTax
        {
            get => _OmitTax;
            set
            {
                _OmitTax = value;
                PriceUpdating();
                PaymentsUpdating();
            }
        }

        /// <summary>
        /// The amount of tax that should be charged for the entire invoice
        /// </summary>
        public decimal TaxAmount => OmitTax ? 0M : Subtotal * TaxRate;

        /// <summary>
        /// The final price of all items on this invoice, after tax
        /// </summary>
        public decimal InvoiceTotal => Subtotal + TaxAmount;

        /// <summary>
        /// The payments currently 'recieved' for this invoice
        /// </summary>
        public decimal PaymentsReceived => Payments.PaidTotal;

        /// <summary>
        /// The difference of the invoice total and payments currently received for this invoice
        /// </summary>
        public decimal PaymentsRequired => InvoiceTotal - PaymentsReceived;

        /// <summary>
        /// True if there is any unpaid balance for this invoice.
        /// </summary>
        public bool InvoiceUnpaid => PaymentsRequired > 0M;

        /// <summary>
        /// Customer "job" name. If null, should use default set in company settings
        /// </summary>
        public string? Customer
        {
            get => _Customer;
            set
            {
                _Customer = string.IsNullOrWhiteSpace(value) ? null : value;
                UpdateInvoiceName();
            }
        }

        public Customer? CustomerJob => Customer != null ? Generations.AccountingIntegration.GetCustomer(Customer) : null;

        /// <summary>
        /// Additional notes/memo for the invoice. Often used for customer contact info, or info for scheduling an item pickup if required
        /// </summary>
        public String AdditonalNotes { get; set; } = string.Empty;

        /// <summary>
        /// A flag additionally indicating that this item requires later customer pickup
        /// </summary>
        public bool FuturePickup { get; set; } = false;

        public bool CommitInvoice()
        {
            try
            {
                Generations.AccountingIntegration.CommitInvoice(this);
                return true;
            } catch (InvoiceCreationException ice)
            {
                // Invoice did not save to accounting software at all
                Logger.Error(ice);
                Generations.Status.Notify(ice.Message);
                return false; // do not close out of invoice
            }
            catch (InvoicePaymentsException ipe)
            {
                // Invoice was created but did not complete a critical step
                Logger.Error(ipe);
                Generations.Status.Notify(ipe.Message);
                return true; // invoice was created, but if payments encountered an error somehow, close the invoice but do notify the user
            } 
            catch (Exception ex)
            {
                Logger.Error(ex);
                Generations.Status.Notify($"An unknown error occured while attempting to commit invoice: {ex.Message} Please do report this issue and you may need to verify in QuickBooks if this invoice was created before closing it in GenerationsPOS.");
                return false;
            }
        }
    }
}
