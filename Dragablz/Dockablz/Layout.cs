using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Dragablz.Core;

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
            EventManager.RegisterClassHandler(typeof (DragablzItem), DragablzItem.PreviewDragDelta, new DragablzDragDeltaEventHandler(PreviewItemDragDelta), true);            
            EventManager.RegisterClassHandler(typeof(DragablzItem), DragablzItem.DragCompleted, new DragablzDragCompletedEventHandler(ItemDragCompleted));
        }

        public Layout()
        {
            Loaded += (sender, args) => LoadedLayouts.Add(this);
            Unloaded += (sender, args) => LoadedLayouts.Remove(this);
        }

        /// <summary>
        /// Use in conjuction with the <see cref="InterTabController.Partition"/> on a <see cref="TabablzControl"/>
        /// to isolate drag and drop spaces/control instances.
        /// </summary>
        public string Partition { get; set; }

        public static readonly DependencyProperty InterLayoutClientProperty = DependencyProperty.Register(
            "InterLayoutClient", typeof (IInterLayoutClient), typeof (Layout), new PropertyMetadata(new DefaultInterLayoutClient()));

        public IInterLayoutClient InterLayoutClient
        {
            get { return (IInterLayoutClient) GetValue(InterLayoutClientProperty); }
            set { SetValue(InterLayoutClientProperty, value); }
        }

        internal static bool IsContainedWithinBranch(DependencyObject dependencyObject)
        {
            do 
            {                
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                if (dependencyObject is Branch)
                    return true;
            } while (dependencyObject != null);
            return false;
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

        public static readonly DependencyProperty BranchTemplateProperty = DependencyProperty.Register(
            "BranchTemplate", typeof (DataTemplate), typeof (Layout), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate BranchTemplate
        {
            get { return (DataTemplate) GetValue(BranchTemplateProperty); }
            set { SetValue(BranchTemplateProperty, value); }
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

        private void Branch(DropZoneLocation location, DragablzItem sourceDragablzItem)
        {
            if (InterLayoutClient == null)
                throw new InvalidOperationException("InterLayoutClient is not set.");

            var sourceOfDragItemsControl = ItemsControl.ItemsControlFromItemContainer(sourceDragablzItem) as DragablzItemsControl;
            if (sourceOfDragItemsControl == null) throw new ApplicationException("Unable to determin source items control.");
            
            var sourceTabControl = TabablzControl.GetOwnerOfHeaderItems(sourceOfDragItemsControl);
            if (sourceTabControl == null) throw new ApplicationException("Unable to determin source tab control.");
            
            var sourceItem = sourceOfDragItemsControl.ItemContainerGenerator.ItemFromContainer(sourceDragablzItem);
            sourceTabControl.RemoveItem(sourceDragablzItem);

            var branchItem = new Branch
            {
                Orientation = (location == DropZoneLocation.Right || location == DropZoneLocation.Left) ? Orientation.Horizontal : Orientation.Vertical
            };

            object newContent;
            if (BranchTemplate == null)
            {
                var newTabHost = InterLayoutClient.GetNewHost(Partition, sourceTabControl);
                if (newTabHost == null)
                    throw new ApplicationException("InterLayoutClient did not provide a new tab host.");
                newTabHost.TabablzControl.AddToSource(sourceItem);
                newTabHost.TabablzControl.SelectedItem = sourceItem;
                newContent = newTabHost.Container;
            }
            else
            {
                newContent = new ContentControl
                {
                    Content = new object(),
                    ContentTemplate = BranchTemplate,                  
                };
                ((ContentControl) newContent).Dispatcher.BeginInvoke(new Action(() =>
                {
                    //TODO might need to improve this a bit, make it a bit more declarative for complex trees
                    var newTabControl = ((ContentControl)newContent).VisualTreeDepthFirstTraversal().OfType<TabablzControl>().FirstOrDefault();                    
                    if (newTabControl != null)
                    {
                        newTabControl.DataContext = sourceTabControl.DataContext;
                        newTabControl.AddToSource(sourceItem);
                        newTabControl.SelectedItem = sourceItem;
                    }
                }), DispatcherPriority.Loaded);                
            }
            
            if (location == DropZoneLocation.Right || location == DropZoneLocation.Bottom)
            {
                branchItem.FirstItem = Content;
                branchItem.SecondItem = newContent;
            }
            else
            {
                branchItem.FirstItem = newContent;
                branchItem.SecondItem = Content;
            }

            SetCurrentValue(ContentProperty, branchItem);            
        }

        internal static void ConsolidateBranch(DependencyObject redundantNode)
        {
            bool isSecondLineageWhenOwnerIsBranch;
            var ownerBranch = FindLayoutOrBranchOwner(redundantNode, out isSecondLineageWhenOwnerIsBranch) as Branch;
            if (ownerBranch == null) return;

            var survivingItem = isSecondLineageWhenOwnerIsBranch ? ownerBranch.FirstItem : ownerBranch.SecondItem;

            var grandParent = FindLayoutOrBranchOwner(ownerBranch, out isSecondLineageWhenOwnerIsBranch);
            if (grandParent == null) throw new ApplicationException("Unexpected structure, grandparent Layout or Branch not found");

            var layout = grandParent as Layout;
            if (layout != null)
            {
                layout.Content = survivingItem;
                return;
            }

            var branch = (Branch) grandParent;
            if (isSecondLineageWhenOwnerIsBranch)
                branch.SecondItem = survivingItem;
            else
                branch.FirstItem = survivingItem;
        }

        private static object FindLayoutOrBranchOwner(DependencyObject node, out bool isSecondLineageWhenOwnerIsBranch)
        {
            isSecondLineageWhenOwnerIsBranch = false;
            
            var ancestoryStack = new Stack<DependencyObject>();
            do
            {
                ancestoryStack.Push(node);
                node = VisualTreeHelper.GetParent(node);
                if (node is Layout) 
                    return node;
                
                var branch = node as Branch;
                if (branch == null) continue;

                isSecondLineageWhenOwnerIsBranch = ancestoryStack.Contains(branch.SecondContentPresenter);
                return branch;

            } while (node != null);            

            return null;
        }

        private static void ItemDragCompleted(object sender, DragablzDragCompletedEventArgs e)
        {
            foreach (var loadedLayout in LoadedLayouts)
                loadedLayout.IsParticipatingInDrag = false;

            if (_currentlyOfferedDropZone == null || e.DragablzItem.IsDropTargetFound) return;

            _currentlyOfferedDropZone.Item1.Branch(_currentlyOfferedDropZone.Item2.Location, e.DragablzItem);
            _currentlyOfferedDropZone = null;
        }

        private static void PreviewItemDragDelta(object sender, DragablzDragDeltaEventArgs e)
        {
            if (e.Cancel) return;

            foreach (var layout in LoadedLayouts.Where(l => l.IsParticipatingInDrag))
            {                
                var cursorPos = Native.GetCursorPos();
                layout.MonitorDropZones(cursorPos);
            }         
        }        
    }
}