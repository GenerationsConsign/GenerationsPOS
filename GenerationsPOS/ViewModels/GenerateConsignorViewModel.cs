using GenerationsPOS.PointOfSale;

namespace GenerationsPOS.ViewModels
{
    /// <summary>
    /// ViewModel for page to generate consignor-specific sales reports 
    /// </summary>
    internal class GenerateConsignorViewModel : ViewModelBase
    {
        public GenerateConsignorViewModel(IGenerationsCore core) : base(core)
        {
        }
    }
}
