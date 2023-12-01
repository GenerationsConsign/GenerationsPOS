using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.PointOfSale.Invoices
{
    /// <summary>
    /// Enumeration-type class representing the types of reductions that may be selected on an item
    /// </summary>
    public class Reduction
    {
        public static Reduction None = new("None", 0, null);
        public static Reduction First = new("First", 10, "30 Days");
        public static Reduction Second = new("Second", 25, "60 Days");
        public static Reduction Third = new("Third", 40, "90 Days");

        public static IEnumerable<Reduction> PredefinedReductions;

        /// <summary>
        /// Custom price reduction as specified by business
        /// </summary> 
        public Reduction(uint percentage) : this("Custom", percentage, null)
        {
        }

        private Reduction(string name, uint percentage, string? policy)
        {
            Name = name;
            Percentage = percentage;
            ReductionPeriod = policy;
        }

        static Reduction()
        {
            PredefinedReductions = new List<Reduction>()
            {
                None, First, Second, Third
            };
        }

        public bool IsCustom => Name == "Custom";

        /// <summary>
        /// The percentage that this reduction will apply to an item price (ex. 10%) 
        /// </summary>
        public uint Percentage { get; }

        /// <summary>
        /// A description of the policy for this reduction being applied
        /// </summary>
        public string? ReductionPeriod { get; }

        public string Name { get; }

        public string? Description
        {
            get
            {
                if (Percentage == 0)
                {
                    // No discount, entered by user or default
                    return null;
                }
                if (ReductionPeriod == null)
                {
                    // Custom discount amount
                    return $"Manual Item Reduction - {Percentage}% Off";
                }
                // else, predefined reduction template
                return $"{ReductionPeriod} Inventory Reduction - {Percentage}% Off";
            }
        }

        private decimal AsDecimal => (decimal)Percentage / 100;

        /// <summary>
        /// Computes the amount that would be removed from an item's final price
        /// </summary>
        public decimal Discount(decimal basePrice) => AsDecimal * basePrice;

        /// <summary>
        /// Computes the final price of an item with this Reduction applied
        public decimal ReducedPrice(decimal basePrice) => (1 - AsDecimal) * basePrice;
    }
}
