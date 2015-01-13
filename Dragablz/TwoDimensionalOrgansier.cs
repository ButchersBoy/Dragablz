using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Dragablz
{
    public class TwoDimensionalOrgansier : IItemsOrganiser
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

        public Point ConstrainLocation(Size measureBounds, Point desiredLocation, Size itemDesiredSize)
        {
            return new Point(
                Math.Min(Math.Max(desiredLocation.X, 0), measureBounds.Width - itemDesiredSize.Width),
                Math.Min(Math.Max(desiredLocation.Y, 0), measureBounds.Height - itemDesiredSize.Height));
        }

        public Size Measure(Size availableSize, IEnumerable<DragablzItem> items)
        {
            return availableSize;
        }
    }
}