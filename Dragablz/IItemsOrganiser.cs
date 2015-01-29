using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Dragablz
{
    public interface IItemsOrganiser
    {
        void Organise(Size measureBounds, IEnumerable<DragablzItem> items);
        void OrganiseOnMouseDownWithing(Size measureBounds, List<DragablzItem> siblingItems, DragablzItem dragablzItem);
        void OrganiseOnDragStarted(Size measureBounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem);
        void OrganiseOnDrag(Size measureBounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem);
        void OrganiseOnDragCompleted(Size measureBounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem);

        Point ConstrainLocation(Size measureBounds, Point itemCurrentLocation, Size itemCurrentSize,
            Point itemDesiredLocation, Size itemDesiredSize);
        Size Measure(Size availableSize, IEnumerable<DragablzItem> items);
    }
}
