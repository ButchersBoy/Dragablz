namespace DragablzSaveDemo
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// The view model that contains the tabs status.
    /// </summary>
    public class TabsViewModel
    {
        private readonly ObservableCollection<TabContentViewModel> _tabContents;

        /// <summary>
        /// Constructor
        /// </summary>
        public TabsViewModel()
        {
            this._tabContents = new ObservableCollection<TabContentViewModel>();
        }

        /// <summary>
        /// The content of the tabs
        /// </summary>
        public ObservableCollection<TabContentViewModel> TabContents
        {
            get
            {
                return this._tabContents;
            }
        }

        /// <summary>
        /// The factory that creates new viewModels
        /// </summary>
        public static Func<object> NewItemFactory => () => new TabContentViewModel(new TabContentModel(Guid.NewGuid().ToString()));
    }
}