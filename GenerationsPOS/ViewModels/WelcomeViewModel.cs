using CommunityToolkit.Mvvm.Input;
using GenerationsPOS.PointOfSale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
