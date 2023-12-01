using Avalonia.Controls;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Emitters.BaseCommandValues;
using ESCPOS_NET.Utilities;
using GenerationsPOS.Data;
using GenerationsPOS.PointOfSale.Hardware.Metrics;
using GenerationsPOS.PointOfSale.Invoices;
using Interop.QBFC16;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.PointOfSale.Hardware
{
    /// <summary>
    /// EpsonReceiptPrinter containing access to the physical Epson hardware
    /// </summary>
    internal class EpsonReceiptPrinter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private SystemStatus Status;
        private IEnumerable<IMetric> PrinterMetrics = new List<IMetric>();

        private readonly string portName;
        private SerialPrinter? _Epson = null;
        private readonly EPSON EPSON = new();

        private byte[]? LogoFile = null;

        public EpsonReceiptPrinter(byte port, SystemStatus status, string? logoPath = null)
        {
            portName = $"COM{port}";
            Status = status;
            Connect();

            if (logoPath != null)
            {
                LoadLogoFile(logoPath);
            }
        }

        #region Epson printer connection
        private SerialPrinter Epson
        {
            get
            {
                if (_Epson == null)
                {
                    logger.Error("EPSON Receipt Printer is not connected! Attempting reconnection");
                    Connect();
                }
                return _Epson;
            }
            set => _Epson = value;
        }

        private void Connect()
        {
            try
            {
                logger.Info("Connecting to Epson Printer via serial port {Port}", portName);
                Epson = new SerialPrinter(portName: portName, baudRate: 115200);
                logger.Info("Connection to Epson Printer via Serial/USB established");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to connect to USB printer");
            }
            CreateMetrics();
        }

        private void HandleDisconnection()
        {
            // Handle reconnection if needed
            throw new NotImplementedException();
        }

        public bool Connected => _Epson?.Status?.IsPrinterOnline == true;

        public void Reconnect()
        {
            _Epson?.Dispose();
            Connect();
        }
        #endregion

        /// <summary>
        /// Sends the drawer open command to via the connected Epson printer
        /// </summary>
        /// <exception cref="IOException">An IOException is thrown if the receipt printer is not conneted</exception>
        public void KickCashDrawer1()
        {
            var printer = Epson ?? throw new IOException("Epson Receipt printer is not connected");
            printer.Write(EPSON.CashDrawerOpenPin2());
        }

        public void LoadLogoFile(string path) => LogoFile = File.ReadAllBytes(path);

        #region Invoice Rendering/Printing
        /// <summary>
        /// Renders a customer invoice and sends to the attached physical Epson printer.
        /// If invoice is null, a test print will be generated.
        /// </summary>
        public void Print(CompanySettings config, CustomerInvoice? invoice)
        {
            var o = EPSON;
            var receiptData = new List<byte[]>();
            void RAdd(params byte[][] data)
            {
                foreach (var line in data)
                {
                    receiptData.Add(line);
                }
            }

            void TAdd(string text) => receiptData.Add(o.PrintLine(text));
            void LineAdd() => receiptData.Add(o.PrintLine(null));

            // Print company logo at top
            RAdd(o.CenterAlign());
            if (LogoFile != null)
            {
                RAdd(o.PrintImage(LogoFile, true));
                LineAdd();
            }

            // Company header/information
            TAdd(config.Header);
            LineAdd();
            RAdd(o.SetStyles(PrintStyle.None));
            
            // Add line items if invoice else test print information
            if (invoice != null)
            {
                // TODO line items
                
                // TODO totals, right aligned

                // TODO customer information
            }
            else
            {
                TAdd("This is a test print. Invoice items will be displayed here.");
            }

            // Company footer/information
            LineAdd();
            TAdd(config.Footer);

            var receiptLines = receiptData.ToArray();
            var receiptOutput = ByteSplicer.Combine(receiptLines);
            Epson.Write(receiptOutput);
        }

        public void TestPrint(CompanySettings config) => Print(config, invoice: null);
        #endregion

        #region Printer metric utilities
        /// <summary>
        /// Creates 'Metric' objects for this receipt printer and registers them through the given status manager
        /// </summary>
        public void CreateMetrics()
        {
            // Unregister metrics if this is a reconnection etc.
            // Printer-detail metrics were not added if the printer was not initially connected.
            Status.RemoveMetrics(PrinterMetrics);
            var metrics = new List<IMetric>();
            PrinterMetrics = metrics;

            // Register 'connected' status regardless of printer connection state (will itself indicate disconnection)
            var connectedMetric = new Metric<bool>(
                name: "Receipt Printer Connected",
                initialState: _Epson != null,
                formatter: c => c ? "Connected" : "Disconnected",
                issuePredicate: c => !c,
                alertIssue: () => Status.NotifyDelayed(1000, "Epson Receipt Printer is disconnected! Please check communication port and printer power. You can retry connection in Application Settings if the link is not restored.")
                );

            metrics.Add(connectedMetric);

            if (_Epson != null)
            {
                _Epson.Connected += (sender, e) =>
                {
                    var args = (ConnectionEventArgs)e;

                    if (connectedMetric.CurrentState != args.IsConnected)
                    {
                        connectedMetric.CurrentState = args.IsConnected;
                    }
                };

                var drawerMetric = new Metric<bool>(
                    name: "Cash Drawer Open",
                    initialState: _Epson.Status.IsCashDrawerOpen ?? false,
                    formatter: o => o ? "Yes" : "No",
                    issuePredicate: _ => false // Cash drawer open does not cause alert 
                    );
                metrics.Add(drawerMetric);

                var printerErrorMetric = new Metric<bool>(
                    name: "Printer Error State",
                    initialState: _Epson.Status.IsInErrorState ?? true,
                    formatter: e => e ? "ERROR" : "Clear",
                    issuePredicate: e => e
                    );
                metrics.Add(printerErrorMetric);

                var coverOpenMetric = new Metric<bool>(
                    name: "Printer Cover Open",
                    initialState: _Epson.Status.IsCoverOpen ?? false,
                    formatter: o => o ? "OPEN" : "Closed",
                    issuePredicate: o => o
                    );
                metrics.Add(coverOpenMetric);

                var printerPaperMetric = new Metric<PaperLevel>(
                    name: "Printer Paper Level",
                    initialState: ParsePaperLevel(),
                    formatter: l => l.ToString(),
                    issuePredicate: l => l != PaperLevel.GOOD
                    );
                metrics.Add(printerPaperMetric);

                // Register status changed event handler to update the above metrics
                _Epson.StatusChanged += (sender, e) =>
                {
                    var args = (PrinterStatusEventArgs)e;

                    var drawerOpen = args.IsCashDrawerOpen ?? false;
                    if (drawerMetric.CurrentState != drawerOpen)
                    {
                        drawerMetric.CurrentState = drawerOpen;
                    }
                    
                    var drawerError = args.IsInErrorState ?? true;
                    if (printerErrorMetric.CurrentState != drawerError)
                    {
                        printerErrorMetric.CurrentState = drawerError;
                    }

                    var coverOpen = args.IsCoverOpen ?? false;
                    if (coverOpenMetric.CurrentState != coverOpen)
                    {
                        coverOpenMetric.CurrentState = coverOpen;
                    }

                    var wasPaperLevelGood = printerPaperMetric.CurrentState == PaperLevel.GOOD;
                    var isPaperLevelGood = args.IsPaperOut == true || args.IsPaperLow == true;
                    if (wasPaperLevelGood != isPaperLevelGood)
                    {
                        printerPaperMetric.CurrentState = ParsePaperLevel();
                    }
                };
            }
            else
            {
                logger.Error("EPSON Printer not connected, not establishing printer metrics");
            }
            Status.AddMetrics(metrics);
        }

        private enum PaperLevel { GOOD, LOW, NONE };

        private PaperLevel ParsePaperLevel()
        {
            if (_Epson?.Status.IsPaperOut == true)
            {
                return PaperLevel.NONE;
            }
            if (_Epson?.Status.IsPaperLow == true)
            {
                return PaperLevel.LOW;
            }
            return PaperLevel.GOOD;
        }
        #endregion
    }
}
