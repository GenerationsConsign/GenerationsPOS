using GenerationsPOS.PointOfSale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.ViewModels
{
    /// <summary>
    /// ViewModel for page to generate store-wide sales reports
    /// </summary>
    internal class GenerateSalesViewModel : ViewModelBase
    {
        public GenerateSalesViewModel(IGenerationsCore core) : base(core)
        {
        }
    }
}
