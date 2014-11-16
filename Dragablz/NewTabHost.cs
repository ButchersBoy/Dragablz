using System;
using System.Windows;

namespace Dragablz
{
    public class NewTabHost : INewTabHost
    {
        private readonly Window _window;
        private readonly TabablzControl _tabablzControl;

        public NewTabHost(Window window, TabablzControl tabablzControl)
        {
            if (window == null) throw new ArgumentNullException("window");
            if (tabablzControl == null) throw new ArgumentNullException("tabablzControl");

            _window = window;
            _tabablzControl = tabablzControl;
        }

        public Window Window
        {
            get { return _window; }
        }

        public TabablzControl TabablzControl
        {
            get { return _tabablzControl; }
        }
    }
}