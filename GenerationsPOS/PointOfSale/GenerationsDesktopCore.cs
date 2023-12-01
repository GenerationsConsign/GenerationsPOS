using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using GenerationsPOS.Data;
using GenerationsPOS.PointOfSale.Accounting;
using GenerationsPOS.PointOfSale.Hardware;
using GenerationsPOS.PointOfSale.Hardware.Metrics;
using GenerationsPOS.PointOfSale.Invoices;
using GenerationsPOS.QuickBooks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenerationsPOS.PointOfSale
{
    /// <summary>
    /// GenerationsCore implementation for the desktop client
    /// </summary>
    public class GenerationsDesktopCore : IGenerationsCore
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private const string LocationFile = "Storage.json";
        private const string DatabaseFile = "GenerationsPOS.db";
        private const string ConfigurationName = "Generations";

        public readonly string DataRoot;
        public readonly string LocalDatabaseFile;

        public IReceiptPrinter ReceiptPrinter { get; private set; }
        public ICashDrawer CashDrawer { get; private set; }
        public DatabaseContext Database { get; private set; }
        public CompanySettings Configuration { get; private set; }
        public UniversalCore Util { get; private set; }
        public InvoiceManager Invoices { get; private set; }
        public SystemStatus Status { get; }
        public IAccounting? AccountingIntegration { get; }

        /// <summary>
        /// Initalizes the components for a full desktop version of GenerationsPOS
        /// </summary>
        public GenerationsDesktopCore(UniversalCore util)
        {
            Util = util;
            // Load the storage root directory from LocationFile:StorageRoot
            try
            {
                var storageConfiguration = new ConfigurationBuilder()
                    .AddJsonFile(LocationFile)
                    .Build();

                DataRoot = storageConfiguration["StorageRoot"]!;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Storage configuration file {Filename} could not be loaded.", LocationFile);
                Environment.Exit(1);
            }
            logger.Info("Initiating GenerationsDesktopCore from storage root {Path}", DataRoot);

            LocalDatabaseFile = Path.Combine(DataRoot, DatabaseFile);

            // Load database
            Database = new DatabaseContext(LocalDatabaseFile);

            // Load or default company settings
            var config = Database.Configuration
                .FirstOrDefault(c => c.Name == ConfigurationName);
            if(config == null)
            {
                // Create new configuration for company
                config = new CompanySettings();
                Database.Configuration
                    .Add(config);
                Database.SaveChanges();
            }
            Configuration = config;

            // Create invoice manager, system status manager
            Invoices = new(this);
            Status = new();

            // Connect to physical thermal receipt printer
            var printer = new EpsonReceiptPrinter((byte)Configuration.COMPort, Status);

            var logoFile = config.LogoFileName;
            try
            {
                if (logoFile != null)
                {
                    var logoPath = Path.Combine(DataRoot, logoFile);
                    printer.LoadLogoFile(logoPath);
                }
            } catch (Exception e)
            {
                logger.Error($"Unable to load company logo file {logoFile}", e);
            }

            // Load implementations for physical thermal receipt printer 
            ReceiptPrinter = new EscPosReceiptPrinter(printer);
            CashDrawer = new EscPosPrinterDrawer(printer);

            // Init QuickBooks integration
            QuickBooksDesktop? qb = null;
            try
            {
                qb = new QuickBooksDesktop(config);
                var qbConnect = Task.Run(() => qb.Connect());
                if (!qbConnect.Wait(TimeSpan.FromSeconds(2)))
                {
                    throw new TimeoutException("Timed out connecting to QuickBooks");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Unable to establish connection to QBXML", ex);
                Status.NotifyDelayed(1000, "Unable to establish connection to QuickBooks Desktop software! You can retry connection in Application Settings or restart this program.");
                Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    Status.Notify("QuickBooks connection will be automatically re-attempted in 10 seconds.");
                    await Task.Delay(10000);
                    if (qb?.IsConnected == false) 
                    {
                        try
                        {
                            qb.Connect();
                        }
                        catch (Exception)
                        {
                            Status.Notify("QuickBooks connection attempt #2 failed. Ensure QuickBooks is open before starting GenerationsPOS.");
                        }
                    }
                });
            }
            AccountingIntegration = qb;

            // Create application shutdown hook to ensure QB session ends (QB will be prevented from closing if sessions are not closed)
            util.AddShutdownHook(() => qb?.Disconnect());
        }
    }
}
