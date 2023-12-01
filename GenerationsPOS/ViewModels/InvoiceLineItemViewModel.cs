using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GenerationsPOS.PointOfSale;
using GenerationsPOS.PointOfSale.Invoices;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.ViewModels
{
    /// <summary>
    /// ViewModel for a single line item on a customer invoice
    /// </summary>
    public partial class InvoiceLineItemViewModel : ViewModelBase
    {
        public InvoiceLineItemViewModel(IGenerationsCore core, InvoiceItem item) : base(core)
        {
            LineItem = item;
        }
        public InvoiceItem LineItem { get; }

        [RelayCommand(CanExecute = nameof(ItemDeletable))]
        public void DeleteLineItem() => LineItem.DeleteLineItem();

        public bool ItemDeletable() => !LineItem.IsLastLineItem();

        /// <summary>
        /// List of all known consignors for autocomplete dropdown
        /// </summary>
        public IEnumerable<string> KnownConsignors
        {
            get => Generations.AccountingIntegration.AllConsignorCodes;
        }

        public IEnumerable<ReductionSet> ReductionTypes => Enum.GetValues<ReductionSet>();

        /// <summary>
        /// The index of the current 'reduction option' from the ReductionSet enum
        /// </summary>
        public int ReductionSelection
        {
            get
            {
                var reduction = LineItem.Reduction;
                var set = Enum.GetValues<ReductionSet>().ToList();
                var matchIndex = set.FindIndex(x => x.ToString() == LineItem.Reduction.Name);
                return matchIndex != -1 ? matchIndex : (int)ReductionSet.Custom;

            }
            set
            {
                var redc = Enum.GetValues<ReductionSet>().ToList()[value];
                var reduction = redc switch
                {
                    ReductionSet.None => Reduction.None,
                    ReductionSet.First => Reduction.First,
                    ReductionSet.Second => Reduction.Second,
                    ReductionSet.Third => Reduction.Third,
                    ReductionSet.Custom => new Reduction(CustomReduction),
                };
                LineItem.Reduction = reduction;
                OnPropertyChanged(nameof(AllowCustomReduction));
            }
        }

        public bool AllowCustomReduction => LineItem.Reduction.IsCustom;

        public uint CustomReduction
        {
            get => LineItem.Reduction.Percentage;
            set
            {
                LineItem.Reduction = new Reduction(value);
            }
        }
    }

    // Enum slightly duplicates listing of reduction types - but easier to work with UI framework with strong enumeration
    public enum ReductionSet
    {
        First,
        Second,
        Third,
        None,
        Custom
    };
}
