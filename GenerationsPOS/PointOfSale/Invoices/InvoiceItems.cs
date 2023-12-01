using Avalonia.Controls.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.PointOfSale.Invoices
{
    /// <summary>
    /// Data class maintaining the line items on a specific customer invoice
    /// </summary>
    public partial class InvoiceItems : ObservableObject
    {
        /// <summary>
        /// Create "new" invoice items list - non-imported line items that will always start with an initial empty line item
        /// </summary>
        public InvoiceItems()
        {
            InsertNewLineItem();
        }

        private BindingList<InvoiceItem> Items { get; } = new();

        public IEnumerable<InvoiceItem> LineItems => from i in Items
                                                     where !string.IsNullOrEmpty(i.ConsignorID) && !string.IsNullOrEmpty(i.ItemName)
                                                     select i;

        /// <summary>
        /// Adds a new event handler to be notified when line items update
        /// </summary>
        public void AddLineItemsUpdateHandler(Action handler) => Items.ListChanged += (sender, e) => handler();

        public int LineItemIndex { get; set; } = -1;

        public void InsertNewLineItem() => Items.Add(new InvoiceItem(this, ++LineItemIndex));

        public void RemoveLineItem(InvoiceItem item) => Items.Remove(item);

        /// <summary>
        /// Applies a transformation to all line items (for display or similar)
        /// </summary>
        public IEnumerable<R> Map<R>(Func<InvoiceItem, R> itemMapper) => Items.Select(itemMapper);

        public decimal ItemTotal => Items.Sum(i => i.FinalPrice);

        public decimal ItemDiscounts => Items.Sum(i => i.Discount);
    }

    /// <summary>
    /// Container for individual line items for a customer invoice 
    /// </summary>
    /// <param name="ConsignorID">Alphanumeric ID for each consignor based on company process (ex. MEI42). Should correlate to an existing Vendor</param>
    /// <param name="ItemName">Item name/description.</param>
    /// <param name="BasePrice">The base/tag price of the item.</param>
    /// <param name="Reduction">The price reduction to be applied (based on company process/policy) to this item</param>
    /// <param name="Quantity">Quantity of this item sold for this specific invoice line item</param>
    public partial class InvoiceItem : ObservableObject
    {
        private readonly InvoiceItems Parent;
        private readonly int InvoiceID;

        private string _ConsignorID;
        private string _ItemName;

        [ObservableProperty]
        private decimal _BasePrice;
        [ObservableProperty]
        private uint _Quantity;

        [ObservableProperty]
        private Reduction _Reduction;

        /// <summary>
        /// Creates a "new" invoice item - with no existing data
        /// </summary>
        public InvoiceItem(InvoiceItems parent, int id) : this(parent, id, string.Empty, string.Empty, 0M, 1)
        {
        }

        /// <summary>
        /// Creates an invoice item with imported data (or defaults)
        /// </summary>
        public InvoiceItem(InvoiceItems parent, int id, string consignor, string description, decimal basePrice, uint quantity)
        {
            Parent = parent;
            InvoiceID = id;
            _ConsignorID = consignor;
            _ItemName = description;
            _BasePrice = basePrice;
            Reduction = Reduction.None;
            Quantity = quantity;
        }

        public string ConsignorID
        {
            get => _ConsignorID;
            set
            {
                _ConsignorID = value.ToUpper();
            }
        }

        public string ItemName
        {
            get => _ItemName;
            set
            {
                _ItemName = value;
                OnLineItemInput();
            }
        }

        public decimal Discount => Reduction.Discount(BasePrice) * Quantity;

        public decimal FinalPrice => Reduction.ReducedPrice(BasePrice) * Quantity;

        public bool IsLastLineItem() => InvoiceID == Parent.LineItemIndex;

        /// <summary>
        /// When a line item has received input into its defining field (consignor ID).
        /// This will create a new line item if the "last" line item is now being entered into.
        /// </summary>
        public void OnLineItemInput()
        {
            if (IsLastLineItem())
            {
                Parent.InsertNewLineItem();
            }
        }

        public void DeleteLineItem()
        {
            if (InvoiceID == Parent.LineItemIndex)
            {
                // Last line item should not be removed, this operation should be disabled at the UI/input level completely.
                // This will prevent a locked out state as the last item will always be empty as a new one will have already been created if not 
                throw new InvalidOperationException("Last line item should not be removed");
            }

            Parent.RemoveLineItem(this);
        }
    }
}
