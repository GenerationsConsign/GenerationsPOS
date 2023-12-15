using GenerationsPOS.PointOfSale;

namespace GenerationsPOS.ViewModels
{
    /// <summary>
    /// ViewModel for page to generate store-wide sales reports
    /// </summary>
    internal class GenerateSalesViewModel : ViewModelBase
    {
        public GenerateSalesViewModel(IGenerationsCore core) : base(core)
        {
        }
    }
}
