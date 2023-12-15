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
