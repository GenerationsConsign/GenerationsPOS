using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using GenerationsPOS.ViewModels;
using GenerationsPOS.PointOfSale;
using System.Threading;
using GenerationsPOS.Views.StatusPane;
using GenerationsPOS.ViewModels.StatusPane;
using System.IO;

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
