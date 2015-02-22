using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Dragablz.Core;

namespace Dragablz
{
    public abstract class StackOrganiser : IItemsOrganiser
    {
        private readonly Orientation _orientation;
        private readonly double _itemOffset;
        private readonly Func<DragablzItem, double> _getDesiredSize;
        private readonly Func<DragablzItem, double> _getLocation;
        private readonly DependencyProperty _canvasDependencyProperty;
        private readonly Action<DragablzItem, double> _setLocation;
        private readonly Dictionary<DragablzItem, double> _activeStoryboardTargetLocations = new Dictionary<DragablzItem, double>();

        protected StackOrganiser(Orientation orientation, double itemOffset = 0)
        {
            _orientation = orientation;
            _itemOffset = itemOffset;

            switch (orientation)
            {
                case Orientation.Horizontal:
                    _getDesiredSize = item => item.DesiredSize.Width;
                    _getLocation = item => item.X;
                    _setLocation = (item, coord) => item.SetCurrentValue(DragablzItem.XProperty, coord);
                    _canvasDependencyProperty = Canvas.LeftProperty;
                    break;
                case Orientation.Vertical:
                    _getDesiredSize = item => item.DesiredSize.Height;
                    _getLocation = item => item.Y;
                    _setLocation = (item, coord) => item.SetCurrentValue(DragablzItem.YProperty, coord);
                    _canvasDependencyProperty = Canvas.TopProperty;
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

        public void Organise(Size measureBounds, IEnumerable<DragablzItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            var currentCoord = 0.0;
            var z = int.MaxValue;
            foreach (
                var newItem in
                    items.Select((di, idx) => new Tuple<int, DragablzItem>(idx, di))
                        .OrderBy(tuple => tuple,
                            MultiComparer<Tuple<int, DragablzItem>>.Ascending(tuple => _getLocation(tuple.Item2))
                                .ThenAscending(tuple => tuple.Item1))
                        .Select(tuple => tuple.Item2))
            {
                Panel.SetZIndex(newItem, newItem.IsSelected ? int.MaxValue : --z);
                SetLocation(newItem, currentCoord);
                newItem.Measure(measureBounds);
                currentCoord += _getDesiredSize(newItem) + _itemOffset;
            }
        }

        public void OrganiseOnMouseDownWithing(Size measureBounds, List<DragablzItem> siblingItems, DragablzItem dragablzItem)
        {
            
        }

        private IDictionary<DragablzItem, LocationInfo> _siblingItemLocationOnDragStart;
        public void OrganiseOnDragStarted(Size measureBounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            if (siblingItems == null) throw new ArgumentNullException("siblingItems");
            if (dragItem == null) throw new ArgumentNullException("dragItem");

            _siblingItemLocationOnDragStart = siblingItems.Select(GetLocationInfo).ToDictionary(loc => loc.Item);
        }

        public void OrganiseOnDrag(Size measureBounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            if (siblingItems == null) throw new ArgumentNullException("siblingItems");
            if (dragItem == null) throw new ArgumentNullException("dragItem");
            
            var currentLocations = siblingItems
                .Select(GetLocationInfo)
                .Union(new [] { GetLocationInfo(dragItem)})
                .OrderBy(loc => loc.Item == dragItem ? loc.Start : _siblingItemLocationOnDragStart[loc.Item].Start);

            var currentCoord = 0.0;
            var zIndex = int.MaxValue;
            foreach (var location in currentLocations)
            {
                if (!Equals(location.Item, dragItem))
                {
                    SendToLocation(location.Item, currentCoord);
                    Panel.SetZIndex(location.Item, --zIndex);
                }
                currentCoord += _getDesiredSize(location.Item) + _itemOffset;
            }
            Panel.SetZIndex(dragItem, int.MaxValue);
        }

        public void OrganiseOnDragCompleted(Size measureBounds, IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            if (siblingItems == null) throw new ArgumentNullException("siblingItems");
            var currentLocations = siblingItems
                .Select(GetLocationInfo)
                .Union(new[] { GetLocationInfo(dragItem) })
                .OrderBy(loc => loc.Item == dragItem ? loc.Start : _siblingItemLocationOnDragStart[loc.Item].Start);

            var currentCoord = 0.0;
            var z = int.MaxValue;
            foreach (var location in currentLocations)
            {
                SetLocation(location.Item, currentCoord);
                currentCoord += _getDesiredSize(location.Item) + _itemOffset;                
                Panel.SetZIndex(location.Item, --z);
            }
            Panel.SetZIndex(dragItem, int.MaxValue);
        }

        public Point ConstrainLocation(Size measureBounds, Point itemCurrentLocation, Size itemCurrentSize, Point itemDesiredLocation, Size itemDesiredSize)
        {
            return new Point(
                _orientation == Orientation.Vertical ? 0 : Math.Min(Math.Max(-1, itemDesiredLocation.X), (measureBounds.Width) + 1),
                _orientation == Orientation.Horizontal ? 0 : Math.Min(Math.Max(-1, itemDesiredLocation.Y), (measureBounds.Height) + 1)
                );
        }

        public Size Measure(Size availableSize, IEnumerable<DragablzItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            var size = new Size(double.PositiveInfinity, double.PositiveInfinity);

            double width = 0, height = 0;
            var isFirst = true;
            foreach (var dragablzItem in items)
            {                
                dragablzItem.Measure(size);
                if (_orientation == Orientation.Horizontal)
                {
                    width += !dragablzItem.IsLoaded ? dragablzItem.DesiredSize.Width : dragablzItem.ActualWidth;
                    if (!isFirst)
                        width += _itemOffset;
                    height = Math.Max(height, !dragablzItem.IsLoaded ? dragablzItem.DesiredSize.Height : dragablzItem.ActualHeight);
                }
                else
                {
                    width = Math.Max(width, !dragablzItem.IsLoaded ? dragablzItem.DesiredSize.Width : dragablzItem.ActualWidth);
                    height += !dragablzItem.IsLoaded ? dragablzItem.DesiredSize.Height : dragablzItem.ActualHeight;
                    if (!isFirst)
                        height += _itemOffset;
                }

                isFirst = false;
            }

            return new Size(Math.Max(width, 0), Math.Max(height, 0));
        }

        private void SetLocation(DragablzItem dragablzItem, double location)
        {                     
            _setLocation(dragablzItem, location);
        }
        
        private void SendToLocation(DragablzItem dragablzItem, double location)
        {                        
            double activeTarget;
            if (Math.Abs(_getLocation(dragablzItem) - location) < 1.0
                ||
                _activeStoryboardTargetLocations.TryGetValue(dragablzItem, out activeTarget)
                && Math.Abs(activeTarget - location) < 1.0)
            {             
                return;
            }            

            _activeStoryboardTargetLocations[dragablzItem] = location;

            var storyboard = new Storyboard {FillBehavior = FillBehavior.Stop};
            storyboard.WhenComplete(sb =>
            {
                _setLocation(dragablzItem, location);
                sb.Remove(dragablzItem);
                _activeStoryboardTargetLocations.Remove(dragablzItem);
            });

            var timeline = new DoubleAnimationUsingKeyFrames();
            timeline.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(_canvasDependencyProperty));
            timeline.KeyFrames.Add(
                new EasingDoubleKeyFrame(location, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)))
                {
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
                });
            storyboard.Children.Add(timeline);            
            storyboard.Begin(dragablzItem, true);            
        }

        private LocationInfo GetLocationInfo(DragablzItem item)
        {
            var size = _getDesiredSize(item);
            double startLocation;
            if (!_activeStoryboardTargetLocations.TryGetValue(item, out startLocation))
                startLocation = _getLocation(item);
            var midLocation = startLocation + size / 2;
            var endLocation = startLocation + size;

            return new LocationInfo(item, startLocation, midLocation, endLocation);
        }
    }
}