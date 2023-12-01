using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GenerationsPOS.Data;
using GenerationsPOS.PointOfSale;
using Microsoft.CodeAnalysis.FlowAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.ViewModels
{
    /// <summary>
    /// ViewModel for page containing company-specific settings that be changed by customer
    /// </summary>
    public partial class CompanySettingsViewModel : ViewModelBase
    {
        private MainViewModel Main;

        private int PrinterPort;
        private string? _CompanyLogo;
        private bool _LightThemeEnabled;

        public CompanySettingsViewModel(MainViewModel main, IGenerationsCore core) : base(core)
        {
            Main = main;

            // load current config into viewmodel
            var cfg = core.Configuration;
            CustomerJob = cfg.DefaultCustomerJobName;
            CashPayment = cfg.CashPaymentType;
            CheckPayment = cfg.CheckPaymentType;
            CardPayment = cfg.CardPaymentType;
            ConsCredit = cfg.ConsignorCreditPaymentType;
            PurchasesAccount = cfg.PurchaseAccount;
            AssetAccount = cfg.AssetAccount;

            Name = cfg.Name;
            CompanyFullName = cfg.CompanyName;
            ReceiptHeader = cfg.Header;
            ReceiptFooter = cfg.Footer;
            _CompanyLogo = cfg.LogoFileName;

            PrinterPort = cfg.COMPort;

            _LightThemeEnabled = Generations.Util.Theme == CompanySettings.Theme.Light;
        }

        // This ViewModel maintains a state of all properties rather than mapping directly to data
        // This allows changes to be dropped and means changes will not be saved until explicitly commanded
        #region QuickBooks Properties
        public string CustomerJob { get; set; }

        public string CashPayment { get; set; }
        
        public string CheckPayment { get; set; }

        public string CardPayment { get; set; }

        public string ConsCredit { get; set; }

        public string PurchasesAccount { get; set; }

        public string AssetAccount { get; set; }

        private bool QuickBooksSessionActive => Generations.AccountingIntegration?.IsConnected == true;

        private bool QuickBooksNoSession => !QuickBooksSessionActive;

        private void UpdateQuickBooksConnectionStatus()
        {
            ReconnectQuickBooksCommand.NotifyCanExecuteChanged();
            DisconnectQuickBooksCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(QuickBooksNoSession))]
        private void ReconnectQuickBooks()
        {
            try
            {
                Generations.AccountingIntegration?.Connect();
                Generations.Status.Notify("QuickBooks software connection established!");
                UpdateQuickBooksConnectionStatus();
            }
            catch (Exception ex)
            {
                logger.Error("Manual QB reconnection failed", ex);
                Generations.Status.Notify("QuickBooks software re-connection failed.");
            }
        }

        [RelayCommand(CanExecute = nameof(QuickBooksSessionActive))]
        private void DisconnectQuickBooks()
        {
            Generations.AccountingIntegration?.Disconnect();
            UpdateQuickBooksConnectionStatus();
        }

        [RelayCommand]
        private void ForceCloseQuickBooks()
        {
            var process = Process.GetProcessesByName("QBW");
            foreach (var p in process)
            {
                p.Kill();
            }
        }
        #endregion

        #region Store/Receipt Info Properties
        public string Name { get; }

        public string CompanyFullName { get; set; }

        public string ReceiptHeader {  get; set; }

        public string ReceiptFooter { get; set; }

        public string CompanyLogo
        {
            get => _CompanyLogo ?? string.Empty;
            set
            {
                if(string.IsNullOrWhiteSpace(value))
                {
                    _CompanyLogo = null;
                }
                else
                {
                    _CompanyLogo = value;
                }
            }
        }

        public static IEnumerable<string> PortOptions => Enumerable.Range(1, 8).Select(p => $"COM{p}");

        public int SelectedPortIndex
        {
            get => PrinterPort - 1;
            set => PrinterPort = value + 1;
        }

        private void UpdatePrinterConnectionStatus()
        {
            ReconnectPrinterCommand.NotifyCanExecuteChanged();
            TestPrintCommand.NotifyCanExecuteChanged();
        }

        private bool PrinterConnected => Generations.ReceiptPrinter.IsConnected;

        private bool PrinterDisconnected => !PrinterConnected;

        [RelayCommand(CanExecute = nameof(PrinterDisconnected))]
        private void ReconnectPrinter()
        {
            Generations.ReceiptPrinter.Reconnect();
            UpdatePrinterConnectionStatus();
        }

        [RelayCommand(CanExecute = nameof(PrinterConnected))]
        private void TestPrint() => Generations.ReceiptPrinter.TestPrint(Generations.Configuration);
        #endregion

        #region Application Properties
        public bool LightThemeEnabled
        {
            get => _LightThemeEnabled;
            set
            {
                _LightThemeEnabled = value;
                Generations.Util.Theme = ThemeSet();
            }
        }

        #endregion

        public void CloseSettings()
        {
            Main.WelcomeWindow();
        }

        public void SaveSettings()
        {
            CommitSettings();
            CloseSettings();
            Generations.Status.Notify("Company settings have been updated.");
        }

        private CompanySettings.Theme ThemeSet() => LightThemeEnabled ? CompanySettings.Theme.Light : CompanySettings.Theme.Dark;
        
        private void CommitSettings()
        {
            // Convert viewmodel back into configuration settings only upon 'save' request
            var cfg = Generations.Configuration;
            cfg.DefaultCustomerJobName = CustomerJob;
            cfg.CashPaymentType = CashPayment;
            cfg.CheckPaymentType = CheckPayment;
            cfg.CardPaymentType = CardPayment;
            cfg.ConsignorCreditPaymentType = ConsCredit;
            cfg.PurchaseAccount = PurchasesAccount;
            cfg.AssetAccount = AssetAccount;

            cfg.CompanyName = CompanyFullName;
            cfg.Header = ReceiptHeader;
            cfg.Footer = ReceiptFooter;
            cfg.LogoFileName = _CompanyLogo;

            cfg.COMPort = PrinterPort;

            cfg.UserTheme = (int)ThemeSet();

            Generations.Database.SaveChanges();
        }
    }
}
