using GenerationsPOS.PointOfSale;

namespace GenerationsPOS.ViewModels;

/// <summary>
/// ViewModel for cash drawer notification popup
/// </summary>
public class NotifyDrawerOpenedViewModel : ViewModelBase
{
    public NotifyDrawerOpenedViewModel(IGenerationsCore core) : base(core)
    {
    }
}
