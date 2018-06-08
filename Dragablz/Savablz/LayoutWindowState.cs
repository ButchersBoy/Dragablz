namespace Dragablz.Savablz
{
    using System.Windows;

    /// <summary>
    /// Represents the state of a window
    /// </summary>
    /// <typeparam name="TTabModel">The type of the tab content model</typeparam>
    public class LayoutWindowState<TTabModel>
    {
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="x">The X position of the window</param>
        /// <param name="y">The Y position of the window</param>
        /// <param name="width">The window width</param>
        /// <param name="height">The window height</param>
        /// <param name="windowState">The window state</param>
        /// <param name="child">The root of this layout</param>
        public LayoutWindowState(double x, double y, double width, double height, WindowState windowState, BranchItemState<TTabModel> child)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.WindowState = windowState;
            this.Child = child;
        }

        /// <summary>
        /// The window's X position
        /// </summary>
        public double X { get; }

        /// <summary>
        /// The window's Y position
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// The window's width
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// The window's height
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// The window state (maximized, restored, minimize)
        /// </summary>
        public WindowState WindowState { get; }

        /// <summary>
        /// The root of this layout
        /// </summary>
        public BranchItemState<TTabModel> Child { get; }
    }
}