using CommunityToolkit.Mvvm.ComponentModel;
using GenerationsPOS.PointOfSale;
using GenerationsPOS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
