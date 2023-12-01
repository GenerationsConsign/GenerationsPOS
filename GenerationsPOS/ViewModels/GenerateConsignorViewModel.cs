using GenerationsPOS.PointOfSale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.ViewModels
{
    /// <summary>
    /// ViewModel for page to generate consignor-specific sales reports 
    /// </summary>
    internal class GenerateConsignorViewModel : ViewModelBase
    {
        public GenerateConsignorViewModel(IGenerationsCore core) : base(core)
        {
        }
    }
}
