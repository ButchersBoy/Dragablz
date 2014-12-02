using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dragablz.Dockablz
{
    public delegate void FloatRequestedEventHandler(object sender, FloatRequestedEventArgs e);

    public class FloatRequestedEventArgs : RoutedEventArgs
    {
        private readonly DragablzItem _dragablzItem;

        public FloatRequestedEventArgs(RoutedEvent routedEvent, object source, DragablzItem dragablzItem) : base(routedEvent, source)
        {
            if (dragablzItem == null) throw new ArgumentNullException("dragablzItem");

            _dragablzItem = dragablzItem;
        }

        public FloatRequestedEventArgs(RoutedEvent routedEvent, DragablzItem dragablzItem) : base(routedEvent)
        {
            if (dragablzItem == null) throw new ArgumentNullException("dragablzItem");

            _dragablzItem = dragablzItem;
        }

        /// <summary>
        /// The source item being dragged which has triggered the float request.
        /// </summary>
        public DragablzItem DragablzItem
        {
            get { return _dragablzItem; }
        }
    }
}
