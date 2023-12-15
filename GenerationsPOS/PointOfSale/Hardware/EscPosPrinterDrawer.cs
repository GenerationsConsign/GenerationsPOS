namespace GenerationsPOS.PointOfSale.Hardware
{
    internal class EscPosPrinterDrawer : ICashDrawer
    {
        private readonly EpsonReceiptPrinter Printer;

        public EscPosPrinterDrawer(EpsonReceiptPrinter printer)
        {
            Printer = printer;
        }

        /// <summary>
        /// Notifes the receipt printer interface to open cash drawer #1
        /// </summary>
        /// <exception cref="IOException">An IOException is thrown if the receipt printer is not conneted</exception>
        public void OpenDrawer()
        {
            Printer.KickCashDrawer1();
        }
    }
}
