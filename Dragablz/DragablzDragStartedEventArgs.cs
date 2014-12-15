using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Dragablz
{
    public delegate void DragablzDragStartedEventHandler(object sender, DragablzDragStartedEventArgs e);

    public class DragablzDragStartedEventArgs : RoutedEventArgs
    {
        private readonly DragablzItem _dragablzItem;
        private readonly DragStartedEventArgs _dragStartedEventArgs;

        public DragablzDragStartedEventArgs(DragablzItem dragablzItem, DragStartedEventArgs dragStartedEventArgs)
        {
            if (dragablzItem == null) throw new ArgumentNullException("dragablzItem");
            if (dragStartedEventArgs == null) throw new ArgumentNullException("dragStartedEventArgs");

            _dragablzItem = dragablzItem;
            _dragStartedEventArgs = dragStartedEventArgs;
        }

        public DragablzDragStartedEventArgs(RoutedEvent routedEvent, DragablzItem dragablzItem, DragStartedEventArgs dragStartedEventArgs)
            : base(routedEvent)
        {
            _dragablzItem = dragablzItem;
            _dragStartedEventArgs = dragStartedEventArgs;
        }

        public DragablzDragStartedEventArgs(RoutedEvent routedEvent, object source, DragablzItem dragablzItem, DragStartedEventArgs dragStartedEventArgs)
            : base(routedEvent, source)
        {
            _dragablzItem = dragablzItem;
            _dragStartedEventArgs = dragStartedEventArgs;
        }

        public DragablzItem DragablzItem
        {
            get { return _dragablzItem; }
        }

        public DragStartedEventArgs DragStartedEventArgs
        {
            get { return _dragStartedEventArgs; }
        }        
    }
}