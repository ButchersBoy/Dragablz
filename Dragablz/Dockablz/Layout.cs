using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dragablz.Core;
using Dragablz.Referenceless;

namespace Dragablz.Dockablz
{
    [TemplatePart(Name = TopDropZonePartName, Type = typeof(DropZone))]
    [TemplatePart(Name = RightDropZonePartName, Type = typeof(DropZone))]
    [TemplatePart(Name = BottomDropZonePartName, Type = typeof(DropZone))]
    [TemplatePart(Name = LeftDropZonePartName, Type = typeof(DropZone))]
    [TemplatePart(Name = CentralDropZonePartName, Type = typeof(DropZone))]
    public class Layout : ContentControl
    {
        private static readonly HashSet<Layout> LoadedLayouts = new HashSet<Layout>();
        private const string TopDropZonePartName = "PART_TopDropZone";
        private const string RightDropZonePartName = "PART_RightDropZone";
        private const string BottomDropZonePartName = "PART_BottomDropZone";
        private const string LeftDropZonePartName = "PART_LeftDropZone";
        private const string CentralDropZonePartName = "PART_CentralDropZone";

        private readonly IDictionary<DropZoneLocation, DropZone> _dropZones = new Dictionary<DropZoneLocation, DropZone>();
        private static Tuple<Layout, DropZone> _currentlyOfferedDropZone;

        static Layout()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Layout), new FrameworkPropertyMetadata(typeof(Layout)));

            EventManager.RegisterClassHandler(typeof(DragablzItem), DragablzItem.DragStarted, new DragablzDragStartedEventHandler(ItemDragStarted));
            EventManager.RegisterClassHandler(typeof (DragablzItem), DragablzItem.PreviewDragDelta, new DragablzDragDeltaEventHandler(ItemDragDelta), true);
            EventManager.RegisterClassHandler(typeof(DragablzItem), DragablzItem.DragCompleted, new DragablzDragCompletedEventHandler(ItemDragCompleted));
        }

        public Layout()
        {
            Loaded += (sender, args) => LoadedLayouts.Add(this);
            Unloaded += (sender, args) => LoadedLayouts.Remove(this);
        }

        public static readonly DependencyProperty PartitionProperty = DependencyProperty.Register(
            "Partition", typeof (object), typeof (Layout), new PropertyMetadata(default(object)));

        /// <summary>
        /// Use in conjuction with the <see cref="InterTabController.Partition"/> on a <see cref="TabablzControl"/>
        /// to isolate drag and drop spaces/control instances.
        /// </summary>
        public object Partition
        {
            get { return (object) GetValue(PartitionProperty); }
            set { SetValue(PartitionProperty, value); }
        }

        private static readonly DependencyPropertyKey IsParticipatingInDragPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsParticipatingInDrag", typeof (bool), typeof (Layout),
                new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsParticipatingInDragProperty =
            IsParticipatingInDragPropertyKey.DependencyProperty;

        public bool IsParticipatingInDrag
        {
            get { return (bool) GetValue(IsParticipatingInDragProperty); }
            private set { SetValue(IsParticipatingInDragPropertyKey, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _dropZones[DropZoneLocation.Top] = GetTemplateChild(TopDropZonePartName) as DropZone;
            _dropZones[DropZoneLocation.Right] = GetTemplateChild(RightDropZonePartName) as DropZone;
            _dropZones[DropZoneLocation.Bottom] = GetTemplateChild(BottomDropZonePartName) as DropZone;
            _dropZones[DropZoneLocation.Left] = GetTemplateChild(LeftDropZonePartName) as DropZone;
            _dropZones[DropZoneLocation.Central] = GetTemplateChild(CentralDropZonePartName) as DropZone;
        }

        private static void ItemDragStarted(object sender, DragablzDragStartedEventArgs e)
        {               
            var sourceOfDragItemsControl = ItemsControl.ItemsControlFromItemContainer(e.DragablzItem) as DragablzItemsControl;
            if (sourceOfDragItemsControl == null || sourceOfDragItemsControl.Items.Count != 1) return;

            var draggingWindow = Window.GetWindow(e.DragablzItem);
            if (draggingWindow == null) return;            

            foreach (var loadedLayout in LoadedLayouts.Where(l =>
                l.Partition == e.DragablzItem.PartitionAtDragStart &&
                !Equals(Window.GetWindow(l), draggingWindow)))

            {                
                loadedLayout.IsParticipatingInDrag = true;
            }
        }

        private void MonitorDropZones(Point cursorPos)
        {
            var myWindow = Window.GetWindow(this);
            if (myWindow == null) return;

            foreach (var dropZone in _dropZones.Values.Where(dz => dz != null))
            {                
                var pointFromScreen = myWindow.PointFromScreen(cursorPos);
                var pointRelativeToDropZone = myWindow.TranslatePoint(pointFromScreen, dropZone);
                var inputHitTest = dropZone.InputHitTest(pointRelativeToDropZone);
                //TODO better halding when windows are layered over each other
                if (inputHitTest != null)
                {
                    if (_currentlyOfferedDropZone != null)
                        _currentlyOfferedDropZone.Item2.IsOffered = false;
                    dropZone.IsOffered = true;
                    _currentlyOfferedDropZone = new Tuple<Layout, DropZone>(this, dropZone);
                }
                else
                {
                    dropZone.IsOffered = false;
                    if (_currentlyOfferedDropZone != null && _currentlyOfferedDropZone.Item2 == dropZone)
                        _currentlyOfferedDropZone = null;
                }
            }
        }

        private void Branch(DropZoneLocation location, DragablzItem sourceItem)
        {
            var sourceOfDragItemsControl = ItemsControl.ItemsControlFromItemContainer(sourceItem) as DragablzItemsControl;
            if (sourceOfDragItemsControl != null)
            {
                var sourceTabControl = TabablzControl.GetOwnerOfHeaderItems(sourceOfDragItemsControl);
                if (sourceTabControl != null)
                    sourceTabControl.RemoveItem(sourceItem);
            }

            var branchItem = new BranchItem
            {
                FirstItem = Content,
                SecondItem = sourceItem
            };
            SetCurrentValue(ContentProperty, branchItem);            
        }

        private static void ItemDragCompleted(object sender, DragablzDragCompletedEventArgs e)
        {
            foreach (var loadedLayout in LoadedLayouts)
                loadedLayout.IsParticipatingInDrag = false;

            if (_currentlyOfferedDropZone != null)
            {                
                _currentlyOfferedDropZone.Item1.Branch(_currentlyOfferedDropZone.Item2.Location, e.DragablzItem);
            }
        }

        private static void ItemDragDelta(object sender, DragablzDragDeltaEventArgs e)
        {         
            foreach (var layout in LoadedLayouts.Where(l => l.IsParticipatingInDrag))
            {                
                var cursorPos = Native.GetCursorPos();
                layout.MonitorDropZones(cursorPos);
            }         
        }
    }

    public enum DropZoneLocation
    {        
        Top,
        Right,
        Bottom,
        Left,     
        Central
    }

    public class DropZone : Control
    {
        static DropZone()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropZone), new FrameworkPropertyMetadata(typeof(DropZone)));            
        }

        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            "Location", typeof (DropZoneLocation), typeof (DropZone), new PropertyMetadata(default(DropZoneLocation)));

        public DropZoneLocation Location
        {
            get { return (DropZoneLocation) GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        private static readonly DependencyPropertyKey IsOfferedPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsOffered", typeof (bool), typeof (DropZone),
                new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsOfferedProperty =
            IsOfferedPropertyKey.DependencyProperty;

        public bool IsOffered
        {
            get { return (bool) GetValue(IsOfferedProperty); }
            internal set { SetValue(IsOfferedPropertyKey, value); }
        }

    }
}