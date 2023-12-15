using GenerationsPOS.PointOfSale;

namespace GenerationsPOS.ViewModels
{
    /// <summary>
    /// ViewModel for the "welcome" page displayed on application start
    /// </summary>
    internal class WelcomeViewModel : ViewModelBase
    {
        public WelcomeViewModel(IGenerationsCore core) : base(core)
        {
        }
    }
}
