using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.PointOfSale.Hardware
{
    public interface ICashDrawer
    {
        /// <summary>
        /// Open the physical cash drawer attached to this system
        /// </summary>
        public void OpenDrawer();
    }
}
