using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using Dragablz.Core;

namespace Dragablz
{
    /// <summary>
    /// Items control which typically uses a canvas and 
    /// </summary>
    public class DragablzItemsControl : ItemsControl
    {
        private readonly IList<DragablzItem> _itemsPendingInitialArrangement = new List<DragablzItem>();
        private object[] _previousSortQueryResult;

        static DragablzItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragablzItemsControl), new FrameworkPropertyMetadata(typeof(DragablzItemsControl)));            
        }

        public DragablzItemsControl()
        {            
            ItemContainerGenerator.StatusChanged += ItemContainerGeneratorOnStatusChanged;
            ItemContainerGenerator.ItemsChanged += ItemContainerGeneratorOnItemsChanged;
            AddHandler(DragablzItem.XChangedEvent, new RoutedPropertyChangedEventHandler<double>(ItemXChanged));
            AddHandler(DragablzItem.YChangedEvent, new RoutedPropertyChangedEventHandler<double>(ItemYChanged));
            AddHandler(DragablzItem.DragDelta, new DragablzDragDeltaEventHandler(ItemDragDelta));
            AddHandler(DragablzItem.DragCompleted, new DragablzDragCompletedEventHandler(ItemDragCompleted));
            AddHandler(DragablzItem.DragStarted, new DragablzDragStartedEventHandler(ItemDragStarted));
        }

        private void ItemContainerGeneratorOnItemsChanged(object sender, ItemsChangedEventArgs itemsChangedEventArgs)
        {
            //throw new NotImplementedException();
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            ItemsOrganiser.Organise(new Size(ItemsPresenterWidth, ItemsPresenterHeight), DragablzItems());
            var measure = ItemsOrganiser.Measure(DragablzItems());
            ItemsPresenterWidth = measure.Width;
            ItemsPresenterHeight = measure.Height;            
        }        

        public static readonly DependencyProperty ItemsOrganiserProperty = DependencyProperty.Register(
            "ItemsOrganiser", typeof (IItemsOrganiser), typeof (DragablzItemsControl), new PropertyMetadata(default(IItemsOrganiser)));

        public IItemsOrganiser ItemsOrganiser
        {
            get { return (IItemsOrganiser) GetValue(ItemsOrganiserProperty); }
            set { SetValue(ItemsOrganiserProperty, value); }
        }

        public static readonly DependencyProperty PositionMonitorProperty = DependencyProperty.Register(
            "PositionMonitor", typeof (PositionMonitor), typeof (DragablzItemsControl), new PropertyMetadata(default(PositionMonitor)));

        public PositionMonitor PositionMonitor
        {
            get { return (PositionMonitor) GetValue(PositionMonitorProperty); }
            set { SetValue(PositionMonitorProperty, value); }
        }

        private static readonly DependencyPropertyKey ItemsPresenterWidthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "ItemsPresenterWidth", typeof(double), typeof (DragablzItemsControl),
                new PropertyMetadata(default(double)));

        public static readonly DependencyProperty ItemsPresenterWidthProperty =
            ItemsPresenterWidthPropertyKey.DependencyProperty;

        public double ItemsPresenterWidth
        {
            get { return (double) GetValue(ItemsPresenterWidthProperty); }
            private set { SetValue(ItemsPresenterWidthPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey ItemsPresenterHeightPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "ItemsPresenterHeight", typeof (double), typeof (DragablzItemsControl),
                new PropertyMetadata(default(double)));

        public static readonly DependencyProperty ItemsPresenterHeightProperty =
            ItemsPresenterHeightPropertyKey.DependencyProperty;

        public double ItemsPresenterHeight
        {
            get { return (double) GetValue(ItemsPresenterHeightProperty); }
            private set { SetValue(ItemsPresenterHeightPropertyKey, value); }
        }

        private void ItemContainerGeneratorOnStatusChanged(object sender, EventArgs eventArgs)
        {            
            if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;

            InvalidateMeasure();
            //extra kick
            Dispatcher.BeginInvoke(new Action(InvalidateMeasure), DispatcherPriority.Render);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            var dragablzItem = item as DragablzItem;
            if (dragablzItem == null) return false;

            _itemsPendingInitialArrangement.Add(dragablzItem);

            return false;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var result = new DragablzItem();

            _itemsPendingInitialArrangement.Add(result);

            return result;
        }        

        protected override Size MeasureOverride(Size constraint)        
        {
            if (ItemsOrganiser == null) return base.MeasureOverride(constraint);

            if (LockedMeasure.HasValue)
            {
                ItemsPresenterWidth = LockedMeasure.Value.Width;
                ItemsPresenterHeight = LockedMeasure.Value.Height;
                return LockedMeasure.Value;
            }

            var dragablzItems = DragablzItems().ToList();

            var maxConstraint = new Size(double.PositiveInfinity, double.PositiveInfinity);

            ItemsOrganiser.Organise(maxConstraint, dragablzItems);
            var measure = ItemsOrganiser.Measure(dragablzItems);
            ItemsPresenterWidth = measure.Width;
            ItemsPresenterHeight = measure.Height;

            var width = double.IsInfinity(constraint.Width) ? measure.Width : constraint.Width;
            var height = double.IsInfinity(constraint.Height) ? measure.Height : constraint.Height;

            return new Size(width, height);
        }

        internal void InstigateDrag(object item, Action<DragablzItem> continuation)
        {   
            var dragablzItem = (DragablzItem)ItemContainerGenerator.ContainerFromItem(item);            
            dragablzItem.InstigateDrag(continuation);            
        }

        internal IEnumerable<DragablzItem> DragablzItems()
        {
            return this.Containers<DragablzItem>();            
        }

        internal Size? LockedMeasure { get; set; }

        private void ItemDragStarted(object sender, DragablzDragStartedEventArgs dragablzDragStartedEventArgs)
        {            
            foreach (var dragableItem in DragablzItems()
                .Except(new [] { dragablzDragStartedEventArgs.DragablzItem}))
            {
                dragableItem.IsSiblingDragging = true;
            }            
            dragablzDragStartedEventArgs.DragablzItem.IsDragging = true;

            dragablzDragStartedEventArgs.Handled = true;
        }

        private void ItemDragCompleted(object sender, DragablzDragCompletedEventArgs eventArgs)
        {
            var dragablzItems = DragablzItems()
                .Select(i =>
                {
                    i.IsDragging = false;
                    i.IsSiblingDragging = false;
                    return i;
                })
                .ToList();

            if (ItemsOrganiser != null)
            {
                var bounds = new Size(ActualWidth, ActualHeight);
                ItemsOrganiser.OrganiseOnDragCompleted(bounds,
                    dragablzItems.Except(new[] { eventArgs.DragablzItem }),
                    eventArgs.DragablzItem);
            }

            eventArgs.Handled = true;

            if (ItemsOrganiser == null) return;
            var measure = ItemsOrganiser.Measure(dragablzItems);
            ItemsPresenterWidth = measure.Width;
            ItemsPresenterHeight = measure.Height;
        }

        private void ItemDragDelta(object sender, DragablzDragDeltaEventArgs eventArgs)
        {
            var bounds = new Size(ItemsPresenterWidth, ItemsPresenterHeight);
            var desiredLocation = new Point(
                eventArgs.DragablzItem.X + eventArgs.DragDeltaEventArgs.HorizontalChange,
                eventArgs.DragablzItem.Y += eventArgs.DragDeltaEventArgs.VerticalChange
                );
            if (ItemsOrganiser != null)
                desiredLocation = ItemsOrganiser.ConstrainLocation(bounds, desiredLocation, eventArgs.DragablzItem.DesiredSize);

            eventArgs.DragablzItem.X = desiredLocation.X;
            eventArgs.DragablzItem.Y = desiredLocation.Y;

            if (ItemsOrganiser != null)
                ItemsOrganiser.OrganiseOnDrag(
                    bounds,
                    DragablzItems().Except(new[] {eventArgs.DragablzItem}),
                    eventArgs.DragablzItem);
            
            eventArgs.DragablzItem.BringIntoView();

            eventArgs.Handled = true;
        }

        private void ItemXChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
        {
            UpdateMonitor(routedPropertyChangedEventArgs);
        }

        private void ItemYChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
        {
            UpdateMonitor(routedPropertyChangedEventArgs);
        }        

        private void UpdateMonitor(RoutedEventArgs routedPropertyChangedEventArgs)
        {
            if (PositionMonitor == null) return;

            var dragablzItem = (DragablzItem) routedPropertyChangedEventArgs.OriginalSource;

            if (!Equals(ItemsControlFromItemContainer(dragablzItem), this)) return;

            PositionMonitor.OnLocationChanged(new LocationChangedEventArgs(dragablzItem.Content, new Point(dragablzItem.X, dragablzItem.Y)));

            var linearPositionMonitor = PositionMonitor as LinearPositionMonitor;
            if (linearPositionMonitor == null) return;

            var sortedItems = linearPositionMonitor.Sort(this.Containers<DragablzItem>()).Select(di => di.Content).ToArray();
            if (_previousSortQueryResult == null || !_previousSortQueryResult.SequenceEqual(sortedItems))
                linearPositionMonitor.OnOrderChanged(new OrderChangedEventArgs(_previousSortQueryResult, sortedItems));

            _previousSortQueryResult = sortedItems;
        }
    }
}
