using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Dragablz
{
    public interface IItemsOrganiser
    {
        void Organise(Size bounds, IEnumerable<DragablzItem> items);
        void OrganiseOnDrag(Size bounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem);
        void OrganiseOnDragCompleted(Size bounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem);        
        Point ConstrainLocation(Size bounds, Point desiredLocation, Size itemDesiredSize);
        Size Measure(IEnumerable<DragablzItem> items);
    }
}
