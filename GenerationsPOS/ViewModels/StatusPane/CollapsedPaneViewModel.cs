using GenerationsPOS.PointOfSale;

namespace GenerationsPOS.ViewModels.StatusPane
{
    /// <summary>
    /// ViewModel for the 'collapsed status pane containing button to expand the panel
    /// </summary>
    public class CollapsedPaneViewModel : StatusPaneBase
    {
        public CollapsedPaneViewModel(IGenerationsCore core, StatusPaneViewModel pane) : base(core, pane)
        {
        }
    }
}
