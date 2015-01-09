using System;
using System.Windows;

namespace Dragablz
{
    public class ClosingItemCallbackArgs
    {
        private readonly Window _window;
        private readonly TabablzControl _tabablzControl;
        private readonly DragablzItem _dragablzItem;

        public ClosingItemCallbackArgs(Window window, TabablzControl tabablzControl, DragablzItem dragablzItem)
        {
            if (window == null) throw new ArgumentNullException("window");
            if (tabablzControl == null) throw new ArgumentNullException("tabablzControl");
            if (dragablzItem == null) throw new ArgumentNullException("dragablzItem");

            _window = window;
            _tabablzControl = tabablzControl;
            _dragablzItem = dragablzItem;
        }

        public Window Window
        {
            get { return _window; }
        }

        public TabablzControl TabablzControl
        {
            get { return _tabablzControl; }
        }

        public DragablzItem DragablzItem
        {
            get { return _dragablzItem; }
        }

        public bool IsCancelled { get; private set; }

        public void Cancel()
        {
            IsCancelled = true;
        }
    }
}
