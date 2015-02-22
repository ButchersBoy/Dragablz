using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Dragablz
{
    public class CanvasOrganiser : IItemsOrganiser
    {
        public void Organise(Size measureBounds, IEnumerable<DragablzItem> items)
        {
            
        }

        public void OrganiseOnMouseDownWithing(Size measureBounds, List<DragablzItem> siblingItems, DragablzItem dragablzItem)
        {
            var zIndex = int.MaxValue;
            foreach (var source in siblingItems.OrderByDescending(Panel.GetZIndex))
            {
                Panel.SetZIndex(source, --zIndex);

            }
            Panel.SetZIndex(dragablzItem, int.MaxValue);
        }

        public void OrganiseOnDragStarted(Size measureBounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            
        }

        public void OrganiseOnDrag(Size measureBounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            
        }

        public void OrganiseOnDragCompleted(Size measureBounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            
        }

        public Point ConstrainLocation(Size measureBounds, Point itemCurrentLocation, Size itemCurrentSize, Point itemDesiredLocation, Size itemDesiredSize)
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

        public Size Measure(Size availableSize, IEnumerable<DragablzItem> items)
        {
            return availableSize;
        }
    }
}