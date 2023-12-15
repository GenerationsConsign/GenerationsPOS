using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ESCPOS_NET.Emitters.BaseCommandValues;
using GenerationsPOS.Data;
using GenerationsPOS.PointOfSale.Accounting;
using GenerationsPOS.PointOfSale.Invoices;
using Interop.QBFC16;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using NLog;
using SimpleTcp;

namespace GenerationsPOS.QuickBooks
{
    /// <summary>
    /// This class maintains an actual connection to the 'QuickBooks Desktop' API 
    /// </summary>
    public class QuickBooksConnection
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private object SessionLock;
        private QBSessionManager QBManager;

        private readonly CompanySettings CompanySettings;
        private ICustomerRet DefaultCustomer;
        private CompanyAccounts DefaultCompanyAccounts;

        private Dictionary<string, IPaymentMethodRet> QBPaymentMethods;

        /// <summary>
        /// Initalizes a connection to the QuickBooks API
        /// </summary>
        public QuickBooksConnection(CompanySettings config)
        {
            CompanySettings = config;

            SessionActive = false;
            SessionLock = new object();
            QBManager = new();
        }

        public double TaxRate { get; private set; }

        #region Connection Management
        public bool SessionActive { get; private set; }

        public void Disconnect()
        {
            lock (SessionLock)
            {
                if (SessionActive)
                {
                    QBManager.EndSession();
                }
                QBManager.CloseConnection();
                SessionActive = false;
            }
        }

        public void EstablishConnection()
        {
            lock (SessionLock)
            {
                Disconnect();
                QBManager.OpenConnection2("", "GenerationsPOS", ENConnectionType.ctLocalQBDLaunchUI);
            }
        }
        #endregion

        #region Request Creation
        private IMsgSetRequest CreateRequest()
        {
            lock (SessionLock)
            {
                if (!SessionActive)
                {
                    QBManager.BeginSession("", ENOpenMode.omDontCare);
                    SessionActive = true;
                }
            }
            return QBManager.CreateMsgSetRequest("US", 16, 0);
        }

        public void UpdateEntities()
        {
            DefaultCompanyAccounts = new CompanyAccounts()
            {
                AssetAccount = CompanySettings.AssetAccount,
                PurchasesAccount = CompanySettings.PurchaseAccount, 
                IncomeAccount = CompanySettings.IncomeAccount
            };

            QBPaymentMethods = QueryPaymentMethods();
        }

        private R SendSingleRequest<R>(IMsgSetRequest req) where R : IQBBase => SendMultiRequest<R>(req).First();

        private IEnumerable<R> SendMultiRequest<R>(IMsgSetRequest req) where R : IQBBase
        {
            lock(SessionLock)
            {
                IMsgSetResponse responseMsg = QBManager.DoRequests(req);
                IResponseList responses = responseMsg.ResponseList;
                
                for (int i = 0; i < responses.Count; i++)
                {
                    var responseObject = responses.GetAt(i);

                    var message = responseObject.StatusMessage;
                    var code = responseObject.StatusCode;
                    Logger.Error($"QuickBooks API response error: {code} :: {message}");
                    if (code != 0)
                    {
                        Logger.Error($"QuickBooks API response error: {code} :: {message}");
                        throw new IOException($"{code} error from QuickBooks :: {message}");
                    }

                    yield return (R)responseObject.Detail;
                }
            }
        }
        #endregion

        #region Vendor Queries
        public IEnumerable<IVendorRet> QueryVendors(Action<IVendorQuery>? queryMutator)
        {
            IMsgSetRequest req = CreateRequest();
            IVendorQuery vendorQuery = req.AppendVendorQueryRq();
            queryMutator?.Invoke(vendorQuery);

            IVendorRetList vendorObjects = SendSingleRequest<IVendorRetList>(req);

            if (vendorObjects.Count < 1)
            {
                SessionActive = false;
                throw new IOException();
            }

            for (int i = 0; i < vendorObjects.Count; i++)
            {
                yield return vendorObjects.GetAt(i);
            }
        }

        public IEnumerable<IVendorRet> GetAllVendors() => QueryVendors(null);

        public IEnumerable<IVendorRet> FindVendorByNameStartsWith(string start) => QueryVendors(v =>
        {
            var nameFilter = v.ORVendorListQuery.VendorListFilter.ORNameFilter.NameFilter;
            nameFilter.Name.SetValue(start);
            nameFilter.MatchCriterion.SetValue(ENMatchCriterion.mcStartsWith);
        });

        public IVendorRet? FindVendorByName(string name)
        {
            var match = QueryVendors(v =>
            {
                var nameFilter = v.ORVendorListQuery.VendorListFilter.ORNameFilter.NameFilter;
                nameFilter.MatchCriterion.SetValue(ENMatchCriterion.mcContains);
                nameFilter.Name.SetValue(name);
            });
            return match.FirstOrDefault(v => string.Equals(v.Name.GetValue(), name, StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        #region Customer Queries
        public IEnumerable<ICustomerRet> QueryCustomers(Action<ICustomerQuery>? queryMutator)
        {
            IMsgSetRequest req = CreateRequest();
            ICustomerQuery custQuery = req.AppendCustomerQueryRq();
            queryMutator?.Invoke(custQuery);

            var custObjects = SendSingleRequest<ICustomerRetList>(req);

            for (int i = 0; i < custObjects.Count; i++)
            {
                var cust = custObjects.GetAt(i);

                if (cust.Name.GetValue().Equals(CompanySettings.DefaultCustomerJobName))
                {
                    DefaultCustomer = cust;
                    // Obtain tax rate from default customer job
                    var taxEntity = GetTaxCode(cust.ItemSalesTaxRef.ListID.GetValue());
                    TaxRate = taxEntity?.TaxRate?.GetValue() ?? 0;
                }

                yield return cust;
            }

            if (DefaultCustomer == null)
            {
                throw new IOException($"Default customer job {CompanySettings.DefaultCustomerJobName} was not found in QuickBooks.");
            }
        }

        public IEnumerable<ICustomerRet> GetAllCustomerJobs() => QueryCustomers(null);

        public ICustomerRet? FindCustomerByName(string name)
        {
            var match = QueryCustomers(c =>
            {
                var nameFilter = c.ORCustomerListQuery.CustomerListFilter.ORNameFilter.NameFilter;
                nameFilter.MatchCriterion.SetValue(ENMatchCriterion.mcContains);
                nameFilter.Name.SetValue(name);
            });
            return match.FirstOrDefault(c => string.Equals(c.Name.GetValue(), name, StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        #region Invoice Queries
        public IInvoiceRet CreateInvoice(Customer? customerJob, IEnumerable<InvoiceItem> lineItems, bool setTax)
        {
            if (!lineItems.Any())
            {
                throw new InvoiceCreationException("Empty invoice was not sent to QuickBooks.", null);
            }

            IMsgSetRequest req = CreateRequest();
            IInvoiceAdd invoiceAdd = req.AppendInvoiceAddRq();

            var customerId = customerJob?.QB.Id ?? DefaultCustomer.ListID.GetValue();
            invoiceAdd.CustomerRef.ListID.SetValue(customerId);

            invoiceAdd.TxnDate.SetValue(DateTime.Now);
            invoiceAdd.ShipDate.SetValue(DateTime.Now);

            // Add all line items
            foreach (var item in lineItems)
            {
                var lineItem = invoiceAdd.ORInvoiceLineAddList.Append().InvoiceLineAdd;

                // Item may not exist 
                // If this is a new consignor - corrospoding item will not yet exist
                var consignor = FindVendorByName(item.ConsignorID);
                var existing = FindInventoryItem(consignor);
                var inventoryItem = existing ?? CreateInventoryItem(consignor);

                lineItem.ItemRef.ListID.SetValue(inventoryItem.ListID.GetValue());
                lineItem.Desc.SetValue(item.ItemName);
                lineItem.Quantity.SetValue(item.Quantity);
                lineItem.Amount.SetValue((double)item.FinalPrice);
                lineItem.ClassRef.FullName.SetValue(item.ConsignorID);
                ApplyTaxCode(lineItem, setTax);
            }

            return SendSingleRequest<IInvoiceRet>(req);
        } 

        private void ApplyTaxCode(IInvoiceLineAdd item, bool tax)
        {
            var code = tax ? "TAX" : "NON";
            item.SalesTaxCodeRef.FullName.SetValue(code);
        }

        public void ReceivePayment(IInvoiceRet invoice, InvoicePayments payments)
        {
            // Compile request for all payment types

            foreach (var (paymentType, amount) in payments.PaymentsReceived)
            {
                IMsgSetRequest req = CreateRequest();
                // Create payment received request for each payment type
                IReceivePaymentAdd paymentAdd = req.AppendReceivePaymentAddRq();
                paymentAdd.CustomerRef.ListID.SetValue(invoice.CustomerRef.ListID.GetValue());

                // Set reference to correct payment method
                var paymentMethod = QBPaymentMethods[paymentType.QBName(CompanySettings)];
                paymentAdd.PaymentMethodRef.ListID.SetValue(paymentMethod.ListID.GetValue());

                paymentAdd.TxnDate.SetValue(DateTime.Now);

                paymentAdd.TotalAmount.SetValue(amount);

                // Apply payment to specific invoice amount
                var applyToTxn = paymentAdd.ORApplyPayment.AppliedToTxnAddList.Append();
                applyToTxn.TxnID.SetValue(invoice.TxnID.GetValue());
                applyToTxn.PaymentAmount.SetValue(amount);

                SendSingleRequest<IReceivePaymentRet>(req);
            }
        }
        #endregion

        #region Inventory Item Queries
        public IItemInventoryRet? FindInventoryItem(IVendorRet vendor)
        {
            IMsgSetRequest req = CreateRequest();
            IItemInventoryQuery itemQuery = req.AppendItemInventoryQueryRq();

            itemQuery.ORListQueryWithOwnerIDAndClass.FullNameList.Add(vendor.Name.GetValue());
            var items = SendSingleRequest<IItemInventoryRetList>(req);
            return items != null && items.Count > 0 ? items.GetAt(0) : null;
        }

        public IItemInventoryRet CreateInventoryItem(IVendorRet vendor)
        {
            IMsgSetRequest req = CreateRequest();
            IItemInventoryAdd itemAdd = req.AppendItemInventoryAddRq();

            itemAdd.Name.SetValue(vendor.Name.GetValue());

            // "Purchase" info
            itemAdd.COGSAccountRef.FullName.SetValue(DefaultCompanyAccounts.PurchasesAccount);
            itemAdd.PrefVendorRef.ListID.SetValue(vendor.ListID.GetValue());

            // "Sales" info
            itemAdd.IncomeAccountRef.FullName.SetValue(DefaultCompanyAccounts.IncomeAccount);

            // "Inventory" info
            itemAdd.AssetAccountRef.FullName.SetValue(DefaultCompanyAccounts.AssetAccount);

            var item = SendSingleRequest<IItemInventoryRet>(req);
            return item;
        }
        #endregion

        #region Payment Queries
        public Dictionary<string, IPaymentMethodRet> QueryPaymentMethods()
        {
            // Retrieve all payment methods from QuickBooks
            IMsgSetRequest req = CreateRequest();
            IPaymentMethodQuery _ = req.AppendPaymentMethodQueryRq();
            
            var paymentMethods = SendSingleRequest<IPaymentMethodRetList>(req);
            var methodsByName = new Dictionary<string, IPaymentMethodRet>();
            for (int i = 0; i < paymentMethods.Count; i++)
            {
                var method = paymentMethods.GetAt(i);
                methodsByName[method.Name.GetValue()] = method;
            }
            return methodsByName;
        }

        public IItemSalesTaxRet? GetTaxCode(string qbListId)
        {
            IMsgSetRequest req = CreateRequest();
            IItemSalesTaxQuery taxQuery = req.AppendItemSalesTaxQueryRq();

            taxQuery.ORListQueryWithOwnerIDAndClass.ListIDList.Add(qbListId);

            var codeList = SendSingleRequest<IItemSalesTaxRetList>(req);
            if (codeList != null && codeList.Count > 0)
            {
                return codeList.GetAt(0);
            }
            else
            {
                throw new IOException("Tax code for default customer job is invalid.");
            }
        }
        #endregion
    }
}
