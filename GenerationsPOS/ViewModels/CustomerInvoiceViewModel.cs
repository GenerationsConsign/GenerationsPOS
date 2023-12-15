using CommunityToolkit.Mvvm.Input;
using GenerationsPOS.PointOfSale;
using GenerationsPOS.PointOfSale.Invoices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenerationsPOS.ViewModels
{
    /// <summary>
    /// ViewModel for a single customer invoice
    /// </summary>
    public partial class CustomerInvoiceViewModel : ViewModelBase
    {
        private bool _CashSelected;
        private bool _CheckSelected;
        private bool _CardSelected;
        private bool _ConsCreditSelected;

        public CustomerInvoiceViewModel(IGenerationsCore core, ActiveInvoiceViewModel parent, CustomerInvoice invoice) : base(core)
        {
            InvoiceMenuViewModel = parent;
            Invoice = invoice;

            invoice.LineItems.AddLineItemsUpdateHandler(() =>
            {
                OnPropertyChanged(nameof(LineItemViewModels));
                SaveInvoiceCommand.NotifyCanExecuteChanged();
            });

            CashSelected = CashPayment > 0M;
            CheckSelected = CheckPayment > 0M;
            CardSelected = CardPayment > 0M;
            ConsCreditSelected = ConsCreditPayment > 0M;
        }

        public CustomerInvoice Invoice { get; }

        public ActiveInvoiceViewModel InvoiceMenuViewModel { get; }

        /// <summary>
        /// List of all known customer jobs for autocomplete dropdown
        /// </summary>
        public IEnumerable<string> KnownCustomers
        {
            get => Generations.AccountingIntegration?.AllCustomerNames ?? new List<string>();
        }

        #region Properties for binding payment amounts
        public bool CashSelected
        {
            get => _CashSelected;
            set
            {
                _CashSelected = value;
                if(!value)
                {
                    CashPayment = 0M;
                }
                OnPropertyChanged();
            }
        }

        public decimal CashPayment
        {
            get => Invoice.Payments.GetPayment(PaymentTypes.Cash);
            set
            {
                Invoice.Payments.SetPayment(PaymentTypes.Cash, value);
                OnPropertyChanged();
            }
        }

        public bool CheckSelected
        {
            get => _CheckSelected;
            set
            {
                _CheckSelected = value;
                if (!value)
                {
                    CheckPayment = 0M;
                }
                OnPropertyChanged();
            }
        }

        public decimal CheckPayment
        {
            get => Invoice.Payments.GetPayment(PaymentTypes.Check);
            set
            {
                Invoice.Payments.SetPayment(PaymentTypes.Check, value);
                OnPropertyChanged();
            }
        }

        public bool CardSelected
        {
            get => _CardSelected;
            set
            {
                _CardSelected = value;
                if (!value)
                {
                    CardPayment = 0M;
                }
                OnPropertyChanged();
            }
        }

        public decimal CardPayment
        {
            get => Invoice.Payments.GetPayment(PaymentTypes.Card);
            set
            {
                Invoice.Payments.SetPayment(PaymentTypes.Card, value);
                OnPropertyChanged();
            }
        }

        public bool ConsCreditSelected
        {
            get => _ConsCreditSelected;
            set
            {
                _ConsCreditSelected = value;
                if (!value)
                {
                    ConsCreditPayment = 0M;
                }
                OnPropertyChanged();
            }
        }

        public decimal ConsCreditPayment
        {
            get => Invoice.Payments.GetPayment(PaymentTypes.Consignor);
            set
            {
                Invoice.Payments.SetPayment(PaymentTypes.Consignor, value);
                OnPropertyChanged();  
            }
        }

        public bool OmitTaxChecked
        {
            get => Invoice.OmitTax;
            set => Invoice.OmitTax = value;
        }
        #endregion

        /// <summary>
        /// Generate a viewmodel for each line item from the invoice associated with this view
        /// </summary>
        public IEnumerable<InvoiceLineItemViewModel> LineItemViewModels => Invoice.LineItems.Map(x => new InvoiceLineItemViewModel(Generations, x));

        /// <summary>
        /// Closes a customer invoice (without saving, or any logic) at the request of the customer
        /// Used to cancel edits or abort a canceled sale
        /// </summary>
        public void CloseInvoice() => InvoiceMenuViewModel.CloseInvoice(this);

        /// <summary>
        /// Attempts to save a customer invoice to the accounting software
        /// </summary>
        [RelayCommand(AllowConcurrentExecutions = false, CanExecute = nameof(InvoiceCommitable))]
        private async Task SaveInvoice()
        {
            // Attempt to save this invoice to the accounting software
            bool success = await Task.Run(() => Invoice.CommitInvoice());
            if (success)
            {
                InvoiceMenuViewModel.CloseInvoice(this);
            }
        }

        private bool InvoiceCommitable
        {
            get => Invoice.LineItems.LineItems.Any() && Generations.AccountingIntegration?.IsConnected == true;
        }
    }
}
