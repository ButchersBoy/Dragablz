namespace Dragablz.Savablz
{
    /// <summary>
    /// The item that is stored in a branch state (first or second)
    /// </summary>
    /// <typeparam name="TTabItem">The tab item type</typeparam>
    public class BranchItemState<TTabItem>
    {
        /// <summary>
        /// The generic constructor, used by the Json deserializer
        /// </summary>
        /// <param name="itemAsBranch">If this item is a branch, this parameter must contain the branch state (<c>null</c> otherwise)</param>
        /// <param name="itemAsTabSet">If this item is a tab set, this parameter must contain the tab set state (<c>null</c> otherwise)</param>
        public BranchItemState(BranchState<TTabItem> itemAsBranch, TabSetState<TTabItem> itemAsTabSet)
        {
            this.ItemAsBranch = itemAsBranch;
            this.ItemAsTabSet = itemAsTabSet;
        }

        /// <summary>
        /// The branch, if this item is a branch, <c>null</c> otherwise
        /// </summary>
        public BranchState<TTabItem> ItemAsBranch { get; }

        /// <summary>
        /// The tab set, if this item is a tab set, <c>null</c> otherwise
        /// </summary>
        public TabSetState<TTabItem> ItemAsTabSet { get; }
    }
}