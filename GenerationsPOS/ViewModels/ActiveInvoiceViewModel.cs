using CommunityToolkit.Mvvm.ComponentModel;
using GenerationsPOS.PointOfSale;
using System.Collections.Generic;
using System.Linq;

namespace GenerationsPOS.ViewModels
{
    /// <summary>
    /// ViewModel for page containing tabulated menu of all open invoices 
    /// </summary>
    public partial class ActiveInvoiceViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int _SelectedInvoiceIndex;

        public ActiveInvoiceViewModel(IGenerationsCore core) : base(core)
        {
        }

        /// <summary>
        /// Generate a ViewModel for each active customer invoice
        /// </summary>
        public IEnumerable<CustomerInvoiceViewModel> InvoiceViewModels
        {
            get => Generations.Invoices.ActiveInvoices.Select(x => new CustomerInvoiceViewModel(Generations, this, x));
        }

        /// <summary>
        /// Creates a new customer invoice at the request of the store
        /// </summary>
        public void CreateNewInvoice()
        {
            Generations.Invoices.CreateNewInvoice();
            OnPropertyChanged(nameof(InvoiceViewModels));
            // When creating invoice, navigate to the new/last invoice
            SelectedInvoiceIndex = Generations.Invoices.ActiveInvoices.Count - 1;
        }

        public void CloseInvoice(CustomerInvoiceViewModel invoice)
        {
            var selected = SelectedInvoiceIndex;
            Generations.Invoices.DropInvoice(invoice.Invoice);
            OnPropertyChanged(nameof(InvoiceViewModels));
            // When closing invoice, navigate to the previous invoice
            SelectedInvoiceIndex = selected - 1;
        }
    }
}
