using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationsPOS.PointOfSale.Hardware
{
    internal class RemoteDrawer : ICashDrawer
    {
        public void OpenDrawer()
        {
            Debug.WriteLine("OpenDrawer called for remote application, no operation performed");
        }
    }
}
