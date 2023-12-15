using System.Diagnostics;

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
