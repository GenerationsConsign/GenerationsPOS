using CommunityToolkit.Mvvm.ComponentModel;
using GenerationsPOS.PointOfSale;
using NLog;

namespace GenerationsPOS.ViewModels;

public class ViewModelBase : ObservableObject
{
    protected static Logger logger = LogManager.GetLogger("GenerationsPOS-UI");

    public IGenerationsCore Generations;

    public ViewModelBase(IGenerationsCore core)
    {
        Generations = core;
    }
}
