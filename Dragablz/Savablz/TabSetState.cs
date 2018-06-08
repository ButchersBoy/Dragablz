namespace Dragablz.Savablz
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the state of a TabSet, in a serializable way
    /// </summary>
    /// <typeparam name="TTabModel">The type of the tab content model</typeparam>
    public class TabSetState<TTabModel>
    {
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="selectedTabItemIndex">The index of the tab item that is currently selected in the TabSet</param>
        /// <param name="tabItems">The tab items</param>
        public TabSetState(int? selectedTabItemIndex, IEnumerable<TTabModel> tabItems)
        {
            this.SelectedTabItemIndex = selectedTabItemIndex;
            this.TabItems = tabItems.ToArray();
        }

        /// <summary>
        /// The tab item that is currently selected in the TabSet
        /// </summary>
        public int? SelectedTabItemIndex { get; }

        /// <summary>
        /// The tab items
        /// </summary>
        public TTabModel[] TabItems { get; }

    }
}