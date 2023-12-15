using CommunityToolkit.Mvvm.ComponentModel;
using GenerationsPOS.PointOfSale;

namespace GenerationsPOS.ViewModels.StatusPane
{
    /// <summary>
    /// ViewModel for the 'status pane' on the right side of the application
    /// Will switch pane between 'open' state (with OpenPaneView displayed) and collapsed (with CollapsedPaneView displayed)
    /// </summary>
    public partial class StatusPaneViewModel : ViewModelBase
    {
        public int ExpandedWidth { get; } = 300;
        public int CollapsedWidth { get; } = 24;

        // Initalize the 'open' and 'collapsed' status panel views
        private readonly CollapsedPaneViewModel CollapsedSystemPane;
        private readonly OpenPaneViewModel ExpandedSystemPane;

        public StatusPaneViewModel(IGenerationsCore core) : base(core)
        {
            Generations = core;
            CollapsedSystemPane = new(core, this);
            ExpandedSystemPane = new(core, this);
            CurrentPaneState = CollapsedSystemPane;

            // Inform the core about the method of notifying the user upon alerts
            core.Status.Alert += OpenNotificationPane;
        }

        // The current view that should be rendered onto the status pane
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PaneOpened))] // update the Pane control state
        private StatusPaneBase _CurrentPaneState;
        
        // The current pane expansion status, bound to the current view being displayed
        public bool PaneOpened
        {
            get => CurrentPaneState == ExpandedSystemPane;
        }

        public void OpenNotificationPane() => CurrentPaneState = ExpandedSystemPane;

        public void CloseNotificationPane() => CurrentPaneState = CollapsedSystemPane;
    }
}
