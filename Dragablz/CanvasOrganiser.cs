using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Dragablz
{
    public class CanvasOrganiser : IItemsOrganiser
    {
        public void Organise(DragablzItemsControl requestor, Size measureBounds, IEnumerable<DragablzItem> items)
        {
            
        }

        public void Organise(DragablzItemsControl requestor, Size measureBounds, IOrderedEnumerable<DragablzItem> items)
        {

        }

        public void OrganiseOnMouseDownWithin(DragablzItemsControl requestor, Size measureBounds, List<DragablzItem> siblingItems, DragablzItem dragablzItem)
        {
            var zIndex = int.MaxValue;
            foreach (var source in siblingItems.OrderByDescending(Panel.GetZIndex))
            {
                Panel.SetZIndex(source, --zIndex);

            }
            Panel.SetZIndex(dragablzItem, int.MaxValue);
        }

        public void OrganiseOnDragStarted(DragablzItemsControl requestor, Size measureBounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            
        }

        public void OrganiseOnDrag(DragablzItemsControl requestor, Size measureBounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            
        }

        public void OrganiseOnDragCompleted(DragablzItemsControl requestor, Size measureBounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            
        }

        public Point ConstrainLocation(DragablzItemsControl requestor, Size measureBounds, Point itemCurrentLocation, Size itemCurrentSize, Point itemDesiredLocation, Size itemDesiredSize)
        {
            //we will stop it pushing beyond the bounds...unless it's already beyond...
            var reduceBoundsWidth = itemCurrentLocation.X + itemCurrentSize.Width > measureBounds.Width
                ? 0
                : itemDesiredSize.Width;
            var reduceBoundsHeight = itemCurrentLocation.Y + itemCurrentSize.Height > measureBounds.Height
                ? 0
                : itemDesiredSize.Height;

            return new Point(
                Math.Min(Math.Max(itemDesiredLocation.X, 0), measureBounds.Width - reduceBoundsWidth),
                Math.Min(Math.Max(itemDesiredLocation.Y, 0), measureBounds.Height - reduceBoundsHeight));
        }

        public Size Measure(DragablzItemsControl requestor, Size availableSize, IEnumerable<DragablzItem> items)
        {
            return availableSize;
        }

        public IEnumerable<DragablzItem> Sort(IEnumerable<DragablzItem> items)
        {
            return items;
        }
    }
}