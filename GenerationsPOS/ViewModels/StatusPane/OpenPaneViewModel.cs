using GenerationsPOS.PointOfSale;
using GenerationsPOS.PointOfSale.Hardware.Metrics;

namespace GenerationsPOS.ViewModels.StatusPane
{

    /// <summary>
    /// ViewModel for the 'opened' status pane containing the status pane segments
    /// </summary>
    public partial class OpenPaneViewModel : StatusPaneBase
    {
        public OpenPaneViewModel(IGenerationsCore core, StatusPaneViewModel pane) : base(core, pane)
        {
        }

        public SystemStatus Status => Generations.Status;
    }
}
