namespace Dragablz.Savablz
{
    using System;
    using System.Windows.Controls;

    /// <summary>
    /// The state of a layout branching
    /// </summary>
    /// <typeparam name="TTabModel">The type of the tab content model</typeparam>
    public class BranchState<TTabModel>
    {
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="firstChild">The first child</param>
        /// <param name="secondChild">The second child</param>
        /// <param name="orientation">The split orientation</param>
        /// <param name="ratio">The split ratio</param>
        public BranchState(BranchItemState<TTabModel> firstChild, BranchItemState<TTabModel> secondChild,
            Orientation orientation, double ratio)
        {
            this.FirstChild = firstChild;
            this.SecondChild = secondChild;
            this.Orientation = orientation;
            this.Ratio = ratio;
        }

        /// <summary>
        /// The first branch
        /// </summary>
        public BranchItemState<TTabModel> FirstChild { get; }

        /// <summary>
        /// The second branch
        /// </summary>
        public BranchItemState<TTabModel> SecondChild { get; }

        /// <summary>
        /// The split orientation
        /// </summary>
        public Orientation Orientation { get; }

        /// <summary>
        /// The split ratio
        /// </summary>
        public double Ratio { get; }
    }
}