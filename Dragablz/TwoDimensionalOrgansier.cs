using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Dragablz
{
    public class TwoDimensionalOrgansier : IItemsOrganiser
    {
        public void Organise(Size bounds, IEnumerable<DragablzItem> items)
        {
            
        }

        public void OrganiseOnMouseDownWithing(Size bounds, List<DragablzItem> siblingItems, DragablzItem dragablzItem)
        {
            var zIndex = int.MaxValue;
            foreach (var source in siblingItems.OrderByDescending(Panel.GetZIndex))
            {
                Panel.SetZIndex(source, --zIndex);

            }
            Panel.SetZIndex(dragablzItem, int.MaxValue);
        }

        public void OrganiseOnDragStarted(Size bounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            
        }

        public void OrganiseOnDrag(Size bounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            
        }

        public void OrganiseOnDragCompleted(Size bounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            
        }

        public Point ConstrainLocation(Size bounds, Point desiredLocation, Size itemDesiredSize)
        {
            return desiredLocation;
        }

        public Size Measure(IEnumerable<DragablzItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            double width = 0.0, height = 0.0;
            foreach (var dragablzItem in items.Where(i => i != null))
            {
                width = Math.Max(width, dragablzItem.DesiredSize.Width);
                height = Math.Max(height, dragablzItem.DesiredSize.Height);
            }

            return new Size(width, height);
        }
    }
}