using Avalonia.Controls.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using GenerationsPOS.PointOfSale;
using GenerationsPOS.PointOfSale.Hardware.Metrics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
