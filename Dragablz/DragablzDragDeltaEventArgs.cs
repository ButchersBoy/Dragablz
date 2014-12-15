using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Dragablz
{
    public delegate void DragablzDragDeltaEventHandler(object sender, DragablzDragDeltaEventArgs e);

    public class DragablzDragDeltaEventArgs : RoutedEventArgs
    {
        private readonly DragablzItem _dragablzItem;
        private readonly DragDeltaEventArgs _dragDeltaEventArgs;

        public DragablzDragDeltaEventArgs(DragablzItem dragablzItem, DragDeltaEventArgs dragDeltaEventArgs)
        {
            if (dragablzItem == null) throw new ArgumentNullException("dragablzItem");
            if (dragDeltaEventArgs == null) throw new ArgumentNullException("dragDeltaEventArgs");

            _dragablzItem = dragablzItem;
            _dragDeltaEventArgs = dragDeltaEventArgs;
        }

        public DragablzDragDeltaEventArgs(RoutedEvent routedEvent, DragablzItem dragablzItem, DragDeltaEventArgs dragDeltaEventArgs) : base(routedEvent)
        {
            _dragablzItem = dragablzItem;
            _dragDeltaEventArgs = dragDeltaEventArgs;
        }

        public DragablzDragDeltaEventArgs(RoutedEvent routedEvent, object source, DragablzItem dragablzItem, DragDeltaEventArgs dragDeltaEventArgs) : base(routedEvent, source)
        {
            _dragablzItem = dragablzItem;
            _dragDeltaEventArgs = dragDeltaEventArgs;
        }

        public DragablzItem DragablzItem
        {
            get { return _dragablzItem; }
        }

        public DragDeltaEventArgs DragDeltaEventArgs
        {
            get { return _dragDeltaEventArgs; }
        }

        public bool Cancel { get; set; }        
    }
}