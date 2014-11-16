using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dragablz
{
    public abstract class LinearOrganiser : IItemsOrganiser
    {
        private readonly Orientation _orientation;
        private readonly Func<DragablzItem, double> _getDesiredSize;
        private readonly Func<DragablzItem, double> _getLocation;
        private readonly Action<DragablzItem, double> _setLocation;

        protected LinearOrganiser(Orientation orientation)
        {
            _orientation = orientation;

            switch (orientation)
            {
                case Orientation.Horizontal:
                    _getDesiredSize = item => item.DesiredSize.Width;
                    _getLocation = item => item.X;
                    _setLocation = (item, coord) => item.X = coord;
                    break;
                case Orientation.Vertical:
                    _getDesiredSize = item => item.DesiredSize.Height;
                    _getLocation = item => item.Y;
                    _setLocation = (item, coord) => item.Y = coord;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("orientation");
            }
        }

        #region LocationInfo

        private class LocationInfo
        {
            private readonly DragablzItem _item;
            private readonly double _start;
            private readonly double _mid;
            private readonly double _end;

            public LocationInfo(DragablzItem item, double start, double mid, double end)
            {
                _item = item;
                _start = start;
                _mid = mid;
                _end = end;
            }

            public double Start
            {
                get { return _start; }
            }

            public double Mid
            {
                get { return _mid; }
            }

            public double End
            {
                get { return _end; }
            }

            public DragablzItem Item
            {
                get { return _item; }
            }
        }

        #endregion

        public Orientation Orientation
        {
            get { return _orientation; }
        }

        public void Organise(Size bounds, IEnumerable<DragablzItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            var currentCoord = 0.0;
            foreach (
                var newItem in
                    items.Select((di, idx) => new Tuple<int, DragablzItem>(idx, di))
                        .OrderBy(tuple => tuple,
                            MultiComparer<Tuple<int, DragablzItem>>.Ascending(tuple => _getLocation(tuple.Item2))
                                .ThenAscending(tuple => tuple.Item1))
                        .Select(tuple => tuple.Item2))
            {
                _setLocation(newItem, currentCoord);
                newItem.Measure(bounds);
                currentCoord += _getDesiredSize(newItem);
            }
        }
        

        public void OrganiseOnDrag(Size bounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            if (siblingItems == null) throw new ArgumentNullException("siblingItems");
            if (dragItem == null) throw new ArgumentNullException("dragItem");
            
            var currentLocations = siblingItems
                .Select(GetLocationInfo)
                .Union(new [] { GetLocationInfo(dragItem)})
                .OrderBy(loc => loc.Start);

            var currentCoord = 0.0;
            var zIndex = 0;
            foreach (var location in currentLocations)
            {
                if (!Equals(location.Item, dragItem))
                {
                    _setLocation(location.Item, currentCoord);
                    Panel.SetZIndex(location.Item, zIndex++);
                }
                currentCoord += _getDesiredSize(location.Item);
            }
            Panel.SetZIndex(dragItem, zIndex);
        }

        public void OrganiseOnDragCompleted(Size bounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            if (siblingItems == null) throw new ArgumentNullException("siblingItems");
            var currentLocations = siblingItems
                .Select(GetLocationInfo)
                .Union(new[] { GetLocationInfo(dragItem) })
                .OrderBy(loc => loc.Start);

            var currentCoord = 0.0;
            foreach (var location in currentLocations)
            {
                _setLocation(location.Item, currentCoord);
                currentCoord += _getDesiredSize(location.Item);
            }
        }

        public Point ConstrainLocation(Size bounds, Point desiredLocation, Size itemDesiredSize)
        {
            return new Point(
                _orientation == Orientation.Vertical ? 0 : Math.Min(Math.Max(-1, desiredLocation.X), (bounds.Width) + 1),
                _orientation == Orientation.Horizontal ? 0 : Math.Min(Math.Max(-1, desiredLocation.Y), (bounds.Height) + 1)
                );
        }

        public Size Measure(IEnumerable<DragablzItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            var size = new Size(double.PositiveInfinity, double.PositiveInfinity);

            double width = 0, height = 0;
            foreach (var dragablzItem in items)
            {
                dragablzItem.Measure(size);
                if (_orientation == Orientation.Horizontal)
                {
                    width += !dragablzItem.IsLoaded ? dragablzItem.DesiredSize.Width : dragablzItem.ActualWidth;
                    height = Math.Max(height, !dragablzItem.IsLoaded ? dragablzItem.DesiredSize.Height : dragablzItem.ActualHeight);
                }
                else
                {
                    width = Math.Max(width, !dragablzItem.IsLoaded ? dragablzItem.DesiredSize.Width : dragablzItem.ActualWidth);
                    height += !dragablzItem.IsLoaded ? dragablzItem.DesiredSize.Height : dragablzItem.ActualHeight;                    
                }                
            }

            return new Size(width, height);
        }

        private LocationInfo GetLocationInfo(DragablzItem item)
        {
            var size = _getDesiredSize(item);
            var startLocation = _getLocation(item);
            var midLocation = startLocation + size / 2;
            var endLocation = startLocation + size;

            return new LocationInfo(item, startLocation, midLocation, endLocation);
        }
    }
}