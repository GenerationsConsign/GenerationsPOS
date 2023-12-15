using GenerationsPOS.PointOfSale;

namespace GenerationsPOS.ViewModels.StatusPane
{
    /// <summary>
    /// Base class for status pane components
    /// </summary>
    public partial class StatusPaneBase : ViewModelBase
    {
        public StatusPaneViewModel StatusPane { get; private set; }

        public StatusPaneBase(IGenerationsCore core, StatusPaneViewModel pane) : base(core)
        {
            StatusPane = pane;
        }
    }
}
