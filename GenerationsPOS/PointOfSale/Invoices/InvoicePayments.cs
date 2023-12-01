using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using GenerationsPOS.Data;
using GenerationsPOS.Utilities;
using Interop.QBFC16;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.PointOfSale.Invoices
{

    /// <summary>
    /// Data class maintaining the status of payments on a specific customer invoice
    /// </summary>
    public class InvoicePayments
    {
        private Dictionary<PaymentType, decimal> AllPayments;

        public InvoicePayments()
        {
            // Map all accepted payment forms for this invoice to a money value - starting with 0 for all
            AllPayments = PaymentTypes.AllTypes.ToDictionary(p => p, _ => 0M);
        }

        public Dictionary<PaymentType, double> PaymentsReceived => AllPayments.Where(p => p.Value > 0).ToDictionary(x => x.Key, x => (double)x.Value);

        public event NotifyEventHandler PaymentsUpdate = delegate { };

        /// <summary>
        /// Totals the value of all payment types 'received'
        /// </summary>
        public decimal PaidTotal => AllPayments.Values.Sum();

        /// <summary>
        /// Produces a value representing 'remaining payment required' from an invoice total
        /// </summary>
        public decimal PaymentRequired(decimal invoiceTotal) => invoiceTotal - PaidTotal;

        /// <summary>
        /// Sets the total payment received from a single payment type. Overwrites any recorded payment for that type (as should be natural for user/POS implementation)
        /// </summary>
        /// <param name="type">The payment type <see cref="PaymentType"/> </param>
        /// <param name="amount">The new amount of this payment type recevied</param>
        public void SetPayment(PaymentType type, decimal amount)
        {
            AllPayments[type] = amount;
            PaymentsUpdate();
        }

        /// <summary>
        /// Returns the money amount associated with a specific payment type on an invoice
        /// </summary>
        public decimal GetPayment(PaymentType type) => AllPayments[type];
    }

    /// <summary>
    /// The accepted forms of payment for the store 
    /// </summary>
    public static class PaymentTypes
    {
        /// <summary>
        /// Initalizes the PaymentTypes container with relevant payment options for this store
        /// </summary>
        /// <param name="config">The CompanySettings instance, to handle changes to the payment type names set by the store</param>
        static PaymentTypes()
        {
            Cash = new PaymentType("Cash", c => c.CashPaymentType);
            Check = new PaymentType("Check", c => c.CheckPaymentType);
            Card = new PaymentType("Credit Card", c => c.CardPaymentType);
            Consignor = new PaymentType("Consignor Credit", c => c.ConsignorCreditPaymentType);

            AllTypes = new List<PaymentType>()
            {
                Cash, Check, Card, Consignor
            };
        }

        public static PaymentType Cash { get; }

        public static PaymentType Check { get; }

        public static PaymentType Card { get; }

        public static PaymentType Consignor { get; }

        public static IEnumerable<PaymentType> AllTypes { get; }
    }

    public delegate string QBNamePath(CompanySettings companySettings);

    public class PaymentType
    {
        private QBNamePath QBNamePath;

        public PaymentType(string name, QBNamePath qBNamePath)
        {
            QBNamePath = qBNamePath;
            FriendlyName = name;
        }

        public string FriendlyName { get; }

        public string QBName(CompanySettings config) => QBNamePath(config);

        public override bool Equals(object? obj)
        {
            return obj is PaymentType type &&
                   FriendlyName == type.FriendlyName;
        }

        public override int GetHashCode() => FriendlyName.GetHashCode();
    }
}
