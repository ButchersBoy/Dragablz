﻿using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dragablz.Core;

namespace Dragablz.Dockablz
{
    [TemplatePart(Name = TopDropZonePartName, Type = typeof(DropZone))]
    [TemplatePart(Name = RightDropZonePartName, Type = typeof(DropZone))]
    [TemplatePart(Name = BottomDropZonePartName, Type = typeof(DropZone))]
    [TemplatePart(Name = LeftDropZonePartName, Type = typeof(DropZone))]
    [TemplatePart(Name = FloatingDropZonePartName, Type = typeof(DropZone))]
    [TemplatePart(Name = FloatingContentPresenterPartName, Type = typeof(ContentPresenter))]
    public class Layout : ContentControl
    {
        private static readonly HashSet<Layout> LoadedLayouts = new HashSet<Layout>();
        private const string TopDropZonePartName = "PART_TopDropZone";
        private const string RightDropZonePartName = "PART_RightDropZone";
        private const string BottomDropZonePartName = "PART_BottomDropZone";
        private const string LeftDropZonePartName = "PART_LeftDropZone";
        private const string FloatingDropZonePartName = "PART_FloatDropZone";
        private const string FloatingContentPresenterPartName = "PART_FloatContentPresenter";
    
        private readonly IDictionary<DropZoneLocation, DropZone> _dropZones = new Dictionary<DropZoneLocation, DropZone>();
        private static Tuple<Layout, DropZone> _currentlyOfferedDropZone;
        
        private readonly DragablzItemsControl _floatingItems;
        private static bool _isDragOpWireUpPending;
        private FloatTransfer _floatTransfer;

        static Layout()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Layout), new FrameworkPropertyMetadata(typeof(Layout)));

            EventManager.RegisterClassHandler(typeof(DragablzItem), DragablzItem.DragStarted, new DragablzDragStartedEventHandler(ItemDragStarted));
            EventManager.RegisterClassHandler(typeof(DragablzItem), DragablzItem.PreviewDragDelta, new DragablzDragDeltaEventHandler(PreviewItemDragDelta), true);            
            EventManager.RegisterClassHandler(typeof(DragablzItem), DragablzItem.DragCompleted, new DragablzDragCompletedEventHandler(ItemDragCompleted));            
        }

        public static RoutedCommand UnfloatCommand = new RoutedCommand();

        public Layout()
        {
            Loaded += (sender, args) => LoadedLayouts.Add(this);
            Unloaded += (sender, args) => LoadedLayouts.Remove(this);

            CommandBindings.Add(new CommandBinding(UnfloatCommand, UnfloatExecuted));

            _floatingItems = new DragablzItemsControl(
                GetFloatingContainerForItemOverride, 
                PrepareFloatingContainerForItemOverride,
                ClearingFloatingContainerForItemOverride);
            
            var floatingItemsSourceBinding = new Binding("FloatingItemsSource") { Source = this };
            _floatingItems.SetBinding(ItemsControl.ItemsSourceProperty, floatingItemsSourceBinding);
            var floatingItemsControlStyleBinding = new Binding("FloatingItemsControlStyle") { Source = this };
            _floatingItems.SetBinding(StyleProperty, floatingItemsControlStyleBinding);
            var floatingItemTemplateBinding = new Binding("FloatingItemTemplate") { Source = this };
            _floatingItems.SetBinding(ItemsControl.ItemTemplateProperty, floatingItemTemplateBinding);
            var floatingItemTemplateSelectorBinding = new Binding("FloatingItemTemplateSelector") { Source = this };
            _floatingItems.SetBinding(ItemsControl.ItemTemplateSelectorProperty, floatingItemTemplateSelectorBinding);            
            var floatingItemContainerStyeBinding = new Binding("FloatingItemContainerStyle") { Source = this };
            _floatingItems.SetBinding(ItemsControl.ItemContainerStyleProperty, floatingItemContainerStyeBinding);
            var floatingItemContainerStyleSelectorBinding = new Binding("FloatingItemContainerStyleSelector") { Source = this };
            _floatingItems.SetBinding(ItemsControl.ItemContainerStyleSelectorProperty, floatingItemContainerStyleSelectorBinding);
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

        public static readonly DependencyProperty IsFloatDropZoneEnabledProperty = DependencyProperty.Register(
            "IsFloatDropZoneEnabled", typeof (bool), typeof (Layout), new PropertyMetadata(default(bool)));

        public bool IsFloatDropZoneEnabled
        {
            get { return (bool) GetValue(IsFloatDropZoneEnabledProperty); }
            set { SetValue(IsFloatDropZoneEnabledProperty, value); }
        }

        public ItemCollection FloatingItems
        {
            get { return _floatingItems.Items; }
        }

        public static readonly DependencyProperty FloatingItemsSourceProperty = DependencyProperty.Register(
            "FloatingItemsSource", typeof (IEnumerable), typeof (Layout), new PropertyMetadata(default(IEnumerable)));

        public IEnumerable FloatingItemsSource
        {
            get { return (IEnumerable) GetValue(FloatingItemsSourceProperty); }
            set { SetValue(FloatingItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty FloatingItemsControlStyleProperty = DependencyProperty.Register(
            "FloatingItemsControlStyle", typeof (Style), typeof (Layout), new PropertyMetadata((Style)null));

        /// <summary>
        /// The style to be applied to the <see cref="DragablzItemsControl"/> which is used to display floating items.
        /// In most scenarios it should be OK to leave this to that applied by the default style.
        /// </summary>
        public Style FloatingItemsControlStyle
        {
            get { return (Style) GetValue(FloatingItemsControlStyleProperty); }
            set { SetValue(FloatingItemsControlStyleProperty, value); }
        }

        public static readonly DependencyProperty FloatingItemContainerStyleProperty = DependencyProperty.Register(
            "FloatingItemContainerStyle", typeof (Style), typeof (Layout), new PropertyMetadata(default(Style)));

        public Style FloatingItemContainerStyle
        {
            get { return (Style) GetValue(FloatingItemContainerStyleProperty); }
            set { SetValue(FloatingItemContainerStyleProperty, value); }
        }

        public static readonly DependencyProperty FloatingItemContainerStyleSelectorProperty = DependencyProperty.Register(
            "FloatingItemContainerStyleSelector", typeof (StyleSelector), typeof (Layout), new PropertyMetadata(new CouldBeHeaderedStyleSelector()));

        public StyleSelector FloatingItemContainerStyleSelector
        {
            get { return (StyleSelector) GetValue(FloatingItemContainerStyleSelectorProperty); }
            set { SetValue(FloatingItemContainerStyleSelectorProperty, value); }
        }

        public static readonly DependencyProperty FloatingItemTemplateProperty = DependencyProperty.Register(
            "FloatingItemTemplate", typeof (DataTemplate), typeof (Layout), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate FloatingItemTemplate
        {
            get { return (DataTemplate) GetValue(FloatingItemTemplateProperty); }
            set { SetValue(FloatingItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty FloatingItemTemplateSelectorProperty = DependencyProperty.Register(
            "FloatingItemTemplateSelector", typeof (DataTemplateSelector), typeof (Layout), new PropertyMetadata(default(DataTemplateSelector)));

        public DataTemplateSelector FloatingItemTemplateSelector
        {
            get { return (DataTemplateSelector) GetValue(FloatingItemTemplateSelectorProperty); }
            set { SetValue(FloatingItemTemplateSelectorProperty, value); }
        }

        public static readonly DependencyProperty FloatingItemHeaderMemberPathProperty = DependencyProperty.Register(
            "FloatingItemHeaderMemberPath", typeof (string), typeof (Layout), new PropertyMetadata(default(string)));

        public string FloatingItemHeaderMemberPath
        {
            get { return (string) GetValue(FloatingItemHeaderMemberPathProperty); }
            set { SetValue(FloatingItemHeaderMemberPathProperty, value); }
        }

        public static readonly DependencyProperty FloatingItemDisplayMemberPathProperty = DependencyProperty.Register(
            "FloatingItemDisplayMemberPath", typeof (string), typeof (Layout), new PropertyMetadata(default(string)));

        public string FloatingItemDisplayMemberPath
        {
            get { return (string) GetValue(FloatingItemDisplayMemberPathProperty); }
            set { SetValue(FloatingItemDisplayMemberPathProperty, value); }
        }

        public static readonly DependencyPropertyKey KeyIsFloatingInLayoutPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "IsFloatingInLayout", typeof (bool), typeof (Layout), new PropertyMetadata(default(bool)));

        private static void SetIsFloatingInLayout(DependencyObject element, bool value)
        {
            element.SetValue(KeyIsFloatingInLayoutPropertyKey, value);
        }

        public static bool GetIsFloatingInLayout(DependencyObject element)
        {
            return (bool)element.GetValue(KeyIsFloatingInLayoutPropertyKey.DependencyProperty);
        }

        public override void OnApplyTemplate()
        {            
            base.OnApplyTemplate();

            var floatingItemsContentPresenter = GetTemplateChild(FloatingContentPresenterPartName) as ContentPresenter;
            if (floatingItemsContentPresenter != null)
                floatingItemsContentPresenter.Content = _floatingItems;

            _dropZones[DropZoneLocation.Top] = GetTemplateChild(TopDropZonePartName) as DropZone;
            _dropZones[DropZoneLocation.Right] = GetTemplateChild(RightDropZonePartName) as DropZone;
            _dropZones[DropZoneLocation.Bottom] = GetTemplateChild(BottomDropZonePartName) as DropZone;
            _dropZones[DropZoneLocation.Left] = GetTemplateChild(LeftDropZonePartName) as DropZone;
            _dropZones[DropZoneLocation.Floating] = GetTemplateChild(FloatingDropZonePartName) as DropZone;
        }                 

        private static void ItemDragStarted(object sender, DragablzDragStartedEventArgs e)
        {
            //we wait until drag is in full flow so we know the partition has been setup by the owning tab control
            _isDragOpWireUpPending = true;            
        }

        private static void SetupParticipatingLayouts(DragablzItem dragablzItem)
        {
            var sourceOfDragItemsControl = ItemsControl.ItemsControlFromItemContainer(dragablzItem) as DragablzItemsControl;
            if (sourceOfDragItemsControl == null || sourceOfDragItemsControl.Items.Count != 1) return;

            var draggingWindow = Window.GetWindow(dragablzItem);
            if (draggingWindow == null) return;

            foreach (var loadedLayout in LoadedLayouts.Where(l =>
                l.Partition == dragablzItem.PartitionAtDragStart &&
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

        private static TabablzControl AssertGetSourceTabControl(DragablzItem dragablzItem)
        {
            var sourceOfDragItemsControl = ItemsControl.ItemsControlFromItemContainer(dragablzItem) as DragablzItemsControl;
            if (sourceOfDragItemsControl == null) throw new ApplicationException("Unable to determine source items control.");

            var sourceTabControl = TabablzControl.GetOwnerOfHeaderItems(sourceOfDragItemsControl);
            if (sourceTabControl == null) throw new ApplicationException("Unable to determine source tab control.");

            return sourceTabControl;
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
            _isDragOpWireUpPending = false;

            foreach (var loadedLayout in LoadedLayouts)
                loadedLayout.IsParticipatingInDrag = false;

            if (_currentlyOfferedDropZone == null || e.DragablzItem.IsDropTargetFound) return;

            var assertGetSourceTabControl = AssertGetSourceTabControl(e.DragablzItem);
            if (assertGetSourceTabControl.Items.Count > 1) return;            

            if (_currentlyOfferedDropZone.Item2.Location == DropZoneLocation.Floating)
                Float(_currentlyOfferedDropZone.Item1, e.DragablzItem);
            else
                _currentlyOfferedDropZone.Item1.Branch(_currentlyOfferedDropZone.Item2.Location, e.DragablzItem);

            _currentlyOfferedDropZone = null;
        }

        private static void Float(Layout layout, DragablzItem dragablzItem)
        {
            //TODO we need eq of IManualInterTabClient here, so consumer can control this op'.

            layout._floatTransfer = FloatTransfer.TakeSnapshot(dragablzItem);

            //remove from source
            var sourceOfDragItemsControl = ItemsControl.ItemsControlFromItemContainer(dragablzItem) as DragablzItemsControl;
            if (sourceOfDragItemsControl == null) throw new ApplicationException("Unable to determin source items control.");            
            var sourceTabControl = TabablzControl.GetOwnerOfHeaderItems(sourceOfDragItemsControl);
            if (sourceTabControl == null) throw new ApplicationException("Unable to determin source tab control.");            
            sourceTabControl.RemoveItem(dragablzItem);
            
            //add to float layer            
            CollectionTeaser collectionTeaser;
            if (CollectionTeaser.TryCreate(layout.FloatingItemsSource, out collectionTeaser))
                collectionTeaser.Add(layout._floatTransfer.Content);
            else
                layout.FloatingItems.Add(layout._floatTransfer.Content);
        }

        private static void PreviewItemDragDelta(object sender, DragablzDragDeltaEventArgs e)
        {
            if (e.Cancel) return;

            if (_isDragOpWireUpPending)
            {
                SetupParticipatingLayouts(e.DragablzItem);
                _isDragOpWireUpPending = false;
            }

            foreach (var layout in LoadedLayouts.Where(l => l.IsParticipatingInDrag))
            {                
                var cursorPos = Native.GetCursorPos();
                layout.MonitorDropZones(cursorPos);
            }         
        }

        private void PrepareFloatingContainerForItemOverride(DependencyObject dependencyObject, object o)
        {
            var headeredDragablzItem = dependencyObject as HeaderedDragablzItem;
            if (headeredDragablzItem == null) return;

            SetIsFloatingInLayout(dependencyObject, true);

            var headerBinding = new Binding(FloatingItemHeaderMemberPath) {Source = o};

            headeredDragablzItem.SetBinding(HeaderedDragablzItem.HeaderContentProperty, headerBinding);

            if (_floatTransfer != null && (o == _floatTransfer.Content || dependencyObject == _floatTransfer.Content))
            {
                //TODO might be nice to allow user a bit of control over sizing

                var dragablzItem = (DragablzItem) dependencyObject;
                dragablzItem.SetCurrentValue(DragablzItem.XProperty, _floatingItems.ActualWidth / 2 - _floatTransfer.Width / 2);
                dragablzItem.SetCurrentValue(DragablzItem.YProperty, _floatingItems.ActualHeight / 2 - _floatTransfer.Height / 2);

                _floatTransfer = null;
            }
        }

        private DragablzItem GetFloatingContainerForItemOverride()
        {
            if (string.IsNullOrWhiteSpace(FloatingItemHeaderMemberPath))
                return new DragablzItem();

            return new HeaderedDragablzItem();
        }

        private static void ClearingFloatingContainerForItemOverride(DependencyObject dependencyObject, object o)
        {
            SetIsFloatingInLayout(dependencyObject, false);
        }

        private void UnfloatExecuted(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            var dragablzItem = executedRoutedEventArgs.Parameter as DragablzItem;
            if (dragablzItem == null) return;
            
            var containsBranch = this.LogicalTreeDepthFirstTraversal().OfType<Branch>().Any();

            var exemplarTab = containsBranch //use Visual Traversal when branches exist 
                ? this.VisualTreeDepthFirstTraversal().OfType<TabablzControl>()
                .FirstOrDefault(t => t.InterTabController != null && t.InterTabController.Partition == Partition)
                : this.LogicalTreeDepthFirstTraversal().OfType<TabablzControl>()
                .FirstOrDefault(t => t.InterTabController != null && t.InterTabController.Partition == Partition);

            if (exemplarTab == null) return;

            //TODO passing the exemplar tab in here isnt ideal, as strictly speaking there isnt one.
            var newTabHost = exemplarTab.InterTabController.InterTabClient.GetNewHost(exemplarTab.InterTabController.InterTabClient,
                exemplarTab.InterTabController.Partition, exemplarTab);
            if (newTabHost == null || newTabHost.TabablzControl == null || newTabHost.Container == null)
                throw new ApplicationException("New tab host was not correctly provided");

            var content = dragablzItem.Content ?? dragablzItem;

            //remove from source
            CollectionTeaser collectionTeaser;
            if (CollectionTeaser.TryCreate(FloatingItemsSource, out collectionTeaser))
                collectionTeaser.Remove(content);
            else
                FloatingItems.Remove(content);

            var myWindow = Window.GetWindow(this);
            if (myWindow == null) throw new ApplicationException("Unable to find owning window.");
            newTabHost.Container.Width = myWindow.RestoreBounds.Width;
            newTabHost.Container.Height = myWindow.RestoreBounds.Height;

            newTabHost.Container.Left = myWindow.Left + 20;
            newTabHost.Container.Top = myWindow.Top + 20;                     

            Dispatcher.BeginInvoke(new Action(() =>            
            {
                newTabHost.TabablzControl.AddToSource(content);
                newTabHost.TabablzControl.SelectedItem = content;
                newTabHost.Container.Show();
                newTabHost.Container.Activate();
            }), DispatcherPriority.DataBind);            
        }
    }
}