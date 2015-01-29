using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dragablz.Core;
using Dragablz.Dockablz;
using Dragablz.Referenceless;

namespace Dragablz
{
    //original code specific to keeping visual tree "alive" sourced from http://stackoverflow.com/questions/12432062/binding-to-itemssource-of-tabcontrol-in-wpf

    /// <summary>
    /// Extended tab control which supports tab repositioning, and drag and drop.  Also 
    /// uses the common WPF technique for pesisting the visual tree across tabs.
    /// </summary>
    [TemplatePart(Name = HeaderItemsControlPartName, Type = typeof(DragablzItemsControl))]
    [TemplatePart(Name = ItemsHolderPartName, Type = typeof(Panel))]
    public class TabablzControl : TabControl
    {
        public const string HeaderItemsControlPartName = "PART_HeaderItemsControl";
        public const string ItemsHolderPartName = "PART_ItemsHolder";

        public static RoutedCommand CloseItemCommand = new RoutedCommand();
        public static RoutedCommand AddItemCommand = new RoutedCommand();

        private static readonly HashSet<TabablzControl> LoadedInstances = new HashSet<TabablzControl>();        

        private Panel _itemsHolder;
        private TabHeaderDragStartInformation _tabHeaderDragStartInformation;
        private object _previousSelection;
        private DragablzItemsControl _dragablzItemsControl;
        private IDisposable _templateSubscription;

        static TabablzControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabablzControl), new FrameworkPropertyMetadata(typeof(TabablzControl)));            
        }

        public TabablzControl()
        {            
            AddHandler(DragablzItem.DragStarted, new DragablzDragStartedEventHandler(ItemDragStarted), true);
            AddHandler(DragablzItem.PreviewDragDelta, new DragablzDragDeltaEventHandler(PreviewItemDragDelta), true);
            AddHandler(DragablzItem.DragDelta, new DragablzDragDeltaEventHandler(ItemDragDelta), true);
            AddHandler(DragablzItem.DragCompleted, new DragablzDragCompletedEventHandler(ItemDragCompleted), true);
            CommandBindings.Add(new CommandBinding(CloseItemCommand, CloseItemHandler));
            CommandBindings.Add(new CommandBinding(AddItemCommand, AddItemHandler));            
            Loaded += (sender, args) => LoadedInstances.Add(this);
            Unloaded += (sender, args) => LoadedInstances.Remove(this);
        }        

        public static readonly DependencyProperty CustomHeaderItemStyleProperty = DependencyProperty.Register(
            "CustomHeaderItemStyle", typeof (Style), typeof (TabablzControl), new PropertyMetadata(default(Style)));

        /// <summary>
        /// Style to apply to header items which are not their own item container (<see cref="TabItem"/>).  Typically items bound via the <see cref="ItemsSource"/> will use this style.
        /// </summary>
        public Style CustomHeaderItemStyle
        {
            get { return (Style) GetValue(CustomHeaderItemStyleProperty); }
            set { SetValue(CustomHeaderItemStyleProperty, value); }
        }

        public static readonly DependencyProperty CustomHeaderItemTemplateProperty = DependencyProperty.Register(
            "CustomHeaderItemTemplate", typeof (DataTemplate), typeof (TabablzControl), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate CustomHeaderItemTemplate
        {
            get { return (DataTemplate) GetValue(CustomHeaderItemTemplateProperty); }
            set { SetValue(CustomHeaderItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty DefaultHeaderItemStyleProperty = DependencyProperty.Register(
            "DefaultHeaderItemStyle", typeof (Style), typeof (TabablzControl), new PropertyMetadata(default(Style)));        

        public Style DefaultHeaderItemStyle
        {
            get { return (Style) GetValue(DefaultHeaderItemStyleProperty); }
            set { SetValue(DefaultHeaderItemStyleProperty, value); }
        }

        public static readonly DependencyProperty AdjacentHeaderItemOffsetProperty = DependencyProperty.Register(
            "AdjacentHeaderItemOffset", typeof (double), typeof (TabablzControl), new PropertyMetadata(default(double), AdjacentHeaderItemOffsetPropertyChangedCallback));

        private static void AdjacentHeaderItemOffsetPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            dependencyObject.SetValue(HeaderItemsOrganiserPropertyKey, new HorizontalOrganiser((double)dependencyPropertyChangedEventArgs.NewValue));
        }

        public double AdjacentHeaderItemOffset
        {
            get { return (double) GetValue(AdjacentHeaderItemOffsetProperty); }
            set { SetValue(AdjacentHeaderItemOffsetProperty, value); }
        }

        private static readonly DependencyPropertyKey HeaderItemsOrganiserPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "HeaderItemsOrganiser", typeof (IItemsOrganiser), typeof (TabablzControl),
                new PropertyMetadata(new HorizontalOrganiser()));

        public static readonly DependencyProperty HeaderItemsOrganiserProperty =
            HeaderItemsOrganiserPropertyKey.DependencyProperty;

        public IItemsOrganiser HeaderItemsOrganiser
        {
            get { return (IItemsOrganiser) GetValue(HeaderItemsOrganiserProperty); }
            private set { SetValue(HeaderItemsOrganiserPropertyKey, value); }
        }


        public static readonly DependencyProperty HeaderPrefixContentProperty = DependencyProperty.Register(
            "HeaderPrefixContent", typeof (object), typeof (TabablzControl), new PropertyMetadata(default(object)));

        public object HeaderPrefixContent
        {
            get { return (object) GetValue(HeaderPrefixContentProperty); }
            set { SetValue(HeaderPrefixContentProperty, value); }
        }

        public static readonly DependencyProperty HeaderPrefixContentStringFormatProperty = DependencyProperty.Register(
            "HeaderPrefixContentStringFormat", typeof (string), typeof (TabablzControl), new PropertyMetadata(default(string)));

        public string HeaderPrefixContentStringFormat
        {
            get { return (string) GetValue(HeaderPrefixContentStringFormatProperty); }
            set { SetValue(HeaderPrefixContentStringFormatProperty, value); }
        }

        public static readonly DependencyProperty HeaderPrefixContentTemplateProperty = DependencyProperty.Register(
            "HeaderPrefixContentTemplate", typeof (DataTemplate), typeof (TabablzControl), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate HeaderPrefixContentTemplate
        {
            get { return (DataTemplate) GetValue(HeaderPrefixContentTemplateProperty); }
            set { SetValue(HeaderPrefixContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty HeaderPrefixContentTemplateSelectorProperty = DependencyProperty.Register(
            "HeaderPrefixContentTemplateSelector", typeof (DataTemplateSelector), typeof (TabablzControl), new PropertyMetadata(default(DataTemplateSelector)));

        public DataTemplateSelector HeaderPrefixContentTemplateSelector
        {
            get { return (DataTemplateSelector) GetValue(HeaderPrefixContentTemplateSelectorProperty); }
            set { SetValue(HeaderPrefixContentTemplateSelectorProperty, value); }
        }

        public static readonly DependencyProperty HeaderSuffixContentProperty = DependencyProperty.Register(
                    "HeaderSuffixContent", typeof(object), typeof(TabablzControl), new PropertyMetadata(default(object)));

        public object HeaderSuffixContent
        {
            get { return (object)GetValue(HeaderSuffixContentProperty); }
            set { SetValue(HeaderSuffixContentProperty, value); }
        }

        public static readonly DependencyProperty HeaderSuffixContentStringFormatProperty = DependencyProperty.Register(
            "HeaderSuffixContentStringFormat", typeof(string), typeof(TabablzControl), new PropertyMetadata(default(string)));

        public string HeaderSuffixContentStringFormat
        {
            get { return (string)GetValue(HeaderSuffixContentStringFormatProperty); }
            set { SetValue(HeaderSuffixContentStringFormatProperty, value); }
        }

        public static readonly DependencyProperty HeaderSuffixContentTemplateProperty = DependencyProperty.Register(
            "HeaderSuffixContentTemplate", typeof(DataTemplate), typeof(TabablzControl), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate HeaderSuffixContentTemplate
        {
            get { return (DataTemplate)GetValue(HeaderSuffixContentTemplateProperty); }
            set { SetValue(HeaderSuffixContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty HeaderSuffixContentTemplateSelectorProperty = DependencyProperty.Register(
            "HeaderSuffixContentTemplateSelector", typeof(DataTemplateSelector), typeof(TabablzControl), new PropertyMetadata(default(DataTemplateSelector)));

        public DataTemplateSelector HeaderSuffixContentTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(HeaderSuffixContentTemplateSelectorProperty); }
            set { SetValue(HeaderSuffixContentTemplateSelectorProperty, value); }
        }

        public static readonly DependencyProperty ShowDefaultCloseButtonProperty = DependencyProperty.Register(
            "ShowDefaultCloseButton", typeof (bool), typeof (TabablzControl), new PropertyMetadata(default(bool)));

        /// <summary>
        /// Indicates whether a default close button should be displayed.  If manually templating the tab header content the close command 
        /// can be called by executing the <see cref="TabablzControl.CloseItemCommand"/> command (typically via a <see cref="Button"/>).
        /// </summary>
        public bool ShowDefaultCloseButton
        {
            get { return (bool) GetValue(ShowDefaultCloseButtonProperty); }
            set { SetValue(ShowDefaultCloseButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowDefaultAddButtonProperty = DependencyProperty.Register(
            "ShowDefaultAddButton", typeof (bool), typeof (TabablzControl), new PropertyMetadata(default(bool)));

        /// <summary>
        /// Indicates whether a default add button should be displayed.  Alternately an add button
        /// could be added in <see cref="HeaderPrefixContent"/> or <see cref="HeaderSuffixContent"/>, utilising 
        /// <see cref="AddItemCommand"/>.
        /// </summary>
        public bool ShowDefaultAddButton
        {
            get { return (bool) GetValue(ShowDefaultAddButtonProperty); }
            set { SetValue(ShowDefaultAddButtonProperty, value); }
        }

        public static readonly DependencyProperty InterTabControllerProperty = DependencyProperty.Register(
            "InterTabController", typeof (InterTabController), typeof (TabablzControl), new PropertyMetadata(null, InterTabControllerPropertyChangedCallback));

        private static void InterTabControllerPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var instance = (TabablzControl)dependencyObject;
            if (dependencyPropertyChangedEventArgs.OldValue != null)
                instance.RemoveLogicalChild(dependencyPropertyChangedEventArgs.OldValue);
            if (dependencyPropertyChangedEventArgs.NewValue != null)
                instance.AddLogicalChild(dependencyPropertyChangedEventArgs.NewValue);
        }

        public InterTabController InterTabController
        {
            get { return (InterTabController) GetValue(InterTabControllerProperty); }
            set { SetValue(InterTabControllerProperty, value); }
        }

        public static readonly DependencyProperty NewItemFactoryProperty = DependencyProperty.Register(
            "NewItemFactory", typeof (Func<object>), typeof (TabablzControl), new PropertyMetadata(default(Func<object>)));

        /// <summary>
        /// Allows a factory to be provided for generating new items. Typically used in conjunction with <see cref="AddItemCommand"/>.
        /// </summary>
        public Func<object> NewItemFactory
        {
            get { return (Func<object>) GetValue(NewItemFactoryProperty); }
            set { SetValue(NewItemFactoryProperty, value); }
        }

        public static readonly DependencyProperty ClosingItemCallbackProperty = DependencyProperty.Register(
            "ClosingItemCallback", typeof(Action<ClosingItemCallbackArgs>), typeof(TabablzControl), new PropertyMetadata(default(Action<ClosingItemCallbackArgs>)));

        /// <summary>
        /// Optionally allows a close item hook to be bound in.  If this propety is provided, the func must return true for the close to continue.
        /// </summary>
        public Action<ClosingItemCallbackArgs> ClosingItemCallback
        {
            get { return (Action<ClosingItemCallbackArgs>)GetValue(ClosingItemCallbackProperty); }
            set { SetValue(ClosingItemCallbackProperty, value); }
        }

        private static readonly DependencyPropertyKey IsDraggingWindowPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsDraggingWindow", typeof (bool), typeof (TabablzControl),
                new PropertyMetadata(default(bool), OnIsDraggingWindowChanged));

        public static readonly DependencyProperty IsDraggingWindowProperty =
            IsDraggingWindowPropertyKey.DependencyProperty;

        public bool IsDraggingWindow
        {
            get { return (bool) GetValue(IsDraggingWindowProperty); }
            private set { SetValue(IsDraggingWindowPropertyKey, value); }
        }

        public static readonly RoutedEvent IsDraggingWindowChangedEvent =
            EventManager.RegisterRoutedEvent(
                "IsDraggingWindowChanged",
                RoutingStrategy.Bubble,
                typeof (RoutedPropertyChangedEventHandler<bool>),
                typeof (TabablzControl));

        public event RoutedPropertyChangedEventHandler<bool> IsDraggingWindowChanged
        {
            add { AddHandler(IsDraggingWindowChangedEvent, value); }
            remove { RemoveHandler(IsDraggingWindowChangedEvent, value); }
        }

        private static void OnIsDraggingWindowChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (TabablzControl) d;
            var args = new RoutedPropertyChangedEventArgs<bool>(
                (bool) e.OldValue,
                (bool) e.NewValue)
            {
                RoutedEvent = IsDraggingWindowChangedEvent
            };
            instance.RaiseEvent(args);
            
        }

        /// <summary>
        /// Temporarily set by the framework if a users drag opration causes a Window to close (e.g if a tab is dragging into another tab).
        /// </summary>
        public static readonly DependencyProperty IsClosingAsPartOfDragOperationProperty = DependencyProperty.RegisterAttached(
            "IsClosingAsPartOfDragOperation", typeof (bool), typeof (TabablzControl), new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.NotDataBindable));

        internal static void SetIsClosingAsPartOfDragOperation(Window element, bool value)
        {
            element.SetValue(IsClosingAsPartOfDragOperationProperty, value);
        }

        public static bool GetIsClosingAsPartOfDragOperation(Window element)
        {
            return (bool) element.GetValue(IsClosingAsPartOfDragOperationProperty);
        }

        public override void OnApplyTemplate()
        {            
            if (_templateSubscription != null)
                _templateSubscription.Dispose();
            _templateSubscription = Disposable.Empty;

            _dragablzItemsControl = GetTemplateChild(HeaderItemsControlPartName) as DragablzItemsControl;
            if (_dragablzItemsControl != null)
            {
                _dragablzItemsControl.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorOnStatusChanged;
                _templateSubscription =
                    Disposable.Create(
                        () =>
                            _dragablzItemsControl.ItemContainerGenerator.StatusChanged -=
                                ItemContainerGeneratorOnStatusChanged);
                if (_dragablzItemsControl.ItemContainerStyleSelector == null) 
                _dragablzItemsControl.ItemContainerStyleSelector = new TabablzItemStyleSelector(DefaultHeaderItemStyle,
                    CustomHeaderItemStyle);
            }

            if (SelectedItem == null)
                SetCurrentValue(SelectedItemProperty, Items.OfType<object>().FirstOrDefault());

            _itemsHolder = GetTemplateChild(ItemsHolderPartName) as Panel;
            UpdateSelectedItem();
            MarkInitialSelection();            

            base.OnApplyTemplate();
        }

        /// <summary>
        /// update the visible child in the ItemsHolder
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0)
                _previousSelection = e.RemovedItems[0];
            else if (e.AddedItems.Count > 0)
                _previousSelection = e.AddedItems[0];
            else
                _previousSelection = null;

            base.OnSelectionChanged(e);
            UpdateSelectedItem();

            if (_dragablzItemsControl == null) return;

            Func<IList, IEnumerable<DragablzItem>> notTabItems =
                l =>
                    l.Cast<object>()
                        .Where(o => !(o is TabItem))
                        .Select(o => _dragablzItemsControl.ItemContainerGenerator.ContainerFromItem(o))
                        .OfType<DragablzItem>();

            foreach (var addedItem in notTabItems(e.AddedItems))
            {
                addedItem.IsSelected = true;
                addedItem.BringIntoView();
            }

            foreach (var removedItem in notTabItems(e.RemovedItems))
            {
                removedItem.IsSelected = false;
            }
        }

        /// <summary>
        /// when the items change we remove any generated panel children and add any new ones as necessary
        /// </summary>
        /// <param name="e"></param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (_itemsHolder == null)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    _itemsHolder.Children.Clear();

                    if (Items.Count > 0)
                    {
                        SelectedItem = base.Items[0];
                        UpdateSelectedItem();
                    }

                    break;

                case NotifyCollectionChangedAction.Add:
                    UpdateSelectedItem();
                    break;

                case NotifyCollectionChangedAction.Remove:
                    
                    if (e.OldItems.Contains(SelectedItem))
                        System.Diagnostics.Debugger.Break();

                    foreach (var item in e.OldItems)
                    {                        
                        var cp = FindChildContentPresenter(item);
                        if (cp != null)
                            _itemsHolder.Children.Remove(cp);
                    }                    

                    UpdateSelectedItem();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException("Replace not implemented yet");
            }
        }

        internal static TabablzControl GetOwnerOfHeaderItems(DragablzItemsControl itemsControl)
        {
            return LoadedInstances.FirstOrDefault(t => Equals(t._dragablzItemsControl, itemsControl));
        }

        private void MarkInitialSelection()
        {
            if (_dragablzItemsControl == null ||
                _dragablzItemsControl.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;

            if (_dragablzItemsControl == null || SelectedItem == null) return;

            var tabItem = SelectedItem as TabItem;
            if (tabItem != null)
            {
                tabItem.SetCurrentValue(IsSelectedProperty, true);
            }            
            var containerFromItem =
                _dragablzItemsControl.ItemContainerGenerator.ContainerFromItem(SelectedItem) as DragablzItem;
            if (containerFromItem != null)
            {
                containerFromItem.SetCurrentValue(DragablzItem.IsSelectedProperty, true);                    
            }            
        }

        private void ItemDragStarted(object sender, DragablzDragStartedEventArgs e)
        {
            if (!IsMyItem(e.DragablzItem)) return;

            //the thumb may steal the user selection, so we will try and apply it manually
            if (_dragablzItemsControl == null) return;

            e.DragablzItem.IsDropTargetFound = false;

            var sourceOfDragItemsControl = ItemsControlFromItemContainer(e.DragablzItem) as DragablzItemsControl;
            if (sourceOfDragItemsControl != null && Equals(sourceOfDragItemsControl, _dragablzItemsControl))
            {               
                var itemsControlOffset = Mouse.GetPosition(_dragablzItemsControl);
                _tabHeaderDragStartInformation = new TabHeaderDragStartInformation(e.DragablzItem, itemsControlOffset.X,
                    itemsControlOffset.Y, e.DragStartedEventArgs.HorizontalOffset, e.DragStartedEventArgs.VerticalOffset);

                foreach (var otherItem in _dragablzItemsControl.Containers<DragablzItem>().Except(e.DragablzItem))                
                    otherItem.IsSelected = false;                
                e.DragablzItem.IsSelected = true;
                e.DragablzItem.PartitionAtDragStart = InterTabController != null ? InterTabController.Partition : null;
                var item = _dragablzItemsControl.ItemContainerGenerator.ItemFromContainer(e.DragablzItem);
                var tabItem = item as TabItem;
                if (tabItem != null)
                    tabItem.IsSelected = true;
                else
                    SelectedItem = item;

                if (ShouldDragWindow(sourceOfDragItemsControl))
                    IsDraggingWindow = true;
            }
        }

        private bool ShouldDragWindow(DragablzItemsControl sourceOfDragItemsControl)
        {
            return (Items.Count == 1
                    && (InterTabController == null || InterTabController.MoveWindowWithSolitaryTabs)
                    && !Layout.IsContainedWithinBranch(sourceOfDragItemsControl));
        }

        private void PreviewItemDragDelta(object sender, DragablzDragDeltaEventArgs e)
        {
            if (_dragablzItemsControl == null) return;

            var sourceOfDragItemsControl = ItemsControlFromItemContainer(e.DragablzItem) as DragablzItemsControl;
            if (sourceOfDragItemsControl == null || !Equals(sourceOfDragItemsControl, _dragablzItemsControl)) return;

            if (!ShouldDragWindow(sourceOfDragItemsControl)) return;

            if (MonitorReentry(e)) return;

            var myWindow = Window.GetWindow(this);
            if (myWindow == null) return;

            if (_interTabTransfer != null)
            {
                var cursorPos = Native.GetCursorPos();
                if (_interTabTransfer.BreachOrientation == Orientation.Vertical)
                {
                    var vector = cursorPos - _interTabTransfer.DragStartWindowOffset;
                    myWindow.Left = vector.X;
                    myWindow.Top = vector.Y;                
                }
                else
                {
                    var offset = e.DragablzItem.TranslatePoint(_interTabTransfer.OriginatorContainer.MouseAtDragStart, myWindow);
                    var borderVector = myWindow.PointToScreen(new Point()) - new Point(myWindow.Left, myWindow.Top);
                    offset.Offset(borderVector.X, borderVector.Y);
                    myWindow.Left = cursorPos.X - offset.X;
                    myWindow.Top = cursorPos.Y - offset.Y;
                }                 
            }
            else
            {
                myWindow.Left += e.DragDeltaEventArgs.HorizontalChange;
                myWindow.Top += e.DragDeltaEventArgs.VerticalChange;
            }

            e.Handled = true;
        }

        private bool MonitorReentry(DragablzDragDeltaEventArgs e)
        {
            var screenMousePosition = _dragablzItemsControl.PointToScreen(Mouse.GetPosition(_dragablzItemsControl));
            
            var otherTabablzControls = LoadedInstances
                .Where(
                    tc =>
                        tc != this && tc.InterTabController != null &&
                        Equals(tc.InterTabController.Partition, InterTabController.Partition))
                .Select(tc =>
                {
                    var topLeft = tc._dragablzItemsControl.PointToScreen(new Point());
                    var bottomRight =
                        tc._dragablzItemsControl.PointToScreen(new Point(tc._dragablzItemsControl.ActualWidth,
                            tc._dragablzItemsControl.ActualHeight));

                    return new {tc, topLeft, bottomRight};
                });


            var target = Native.SortWindowsTopToBottom(Application.Current.Windows.OfType<Window>())
                .Join(otherTabablzControls, w => w, a => Window.GetWindow(a.tc), (w, a) => a)
                .FirstOrDefault(a => new Rect(a.topLeft, a.bottomRight).Contains(screenMousePosition));

            if (target != null)
            {
                var mousePositionOnItem = Mouse.GetPosition(e.DragablzItem);

                var floatingItemSnapShots = this.VisualTreeDepthFirstTraversal()
                    .OfType<Layout>()
                    .SelectMany(l => l.FloatingDragablzItems().Select(FloatingItemSnapShot.Take))
                    .ToList();

                e.DragablzItem.IsDropTargetFound = true;
                var item = RemoveItem(e.DragablzItem);                

                var interTabTransfer = new InterTabTransfer(item, e.DragablzItem, mousePositionOnItem, floatingItemSnapShots);
                e.DragablzItem.IsDragging = false;

                target.tc.ReceiveDrag(interTabTransfer);
                e.Cancel = true;
                
                return true;
            }             
   
            return false;
        }

        internal object RemoveItem(DragablzItem dragablzItem)
        {
            var item = _dragablzItemsControl.ItemContainerGenerator.ItemFromContainer(dragablzItem);
            var contentPresenter = FindChildContentPresenter(item);
            RemoveFromSource(item);
            _itemsHolder.Children.Remove(contentPresenter);
            if (Items.Count == 0)
            {
                var window = Window.GetWindow(this);
                if (window != null &&
                    InterTabController.InterTabClient.TabEmptiedHandler(this, window) == TabEmptiedResponse.CloseWindow)
                {
                    try
                    {
                        SetIsClosingAsPartOfDragOperation(window, true);
                        window.Close();
                    }
                    finally
                    {
                        SetIsClosingAsPartOfDragOperation(window, false);
                    }                    
                }
            }
            return item;
        }

        private void ItemDragCompleted(object sender, DragablzDragCompletedEventArgs e)
        {
            if (!IsMyItem(e.DragablzItem)) return;

            _interTabTransfer = null;
            _dragablzItemsControl.LockedMeasure = null;
            IsDraggingWindow = false;
        }

        private void ItemDragDelta(object sender, DragablzDragDeltaEventArgs e)
        {
            if (!IsMyItem(e.DragablzItem)) return;

            if (_tabHeaderDragStartInformation != null &&
                Equals(_tabHeaderDragStartInformation.DragItem, e.DragablzItem) && 
                InterTabController != null)
            {
                if (InterTabController.InterTabClient == null)                    
                    throw new InvalidOperationException("An InterTabClient must be provided on an InterTabController.");
                
                MonitorBreach(e);
            }
        }

        private bool IsMyItem(DragablzItem item)
        {
            return _dragablzItemsControl.DragablzItems().Contains(item);
        }

        private void MonitorBreach(DragablzDragDeltaEventArgs e)
        {
            var mousePosition = Mouse.GetPosition(_dragablzItemsControl);

            Orientation? breachOrientation = null;
            if (mousePosition.X < -InterTabController.HorizontalPopoutGrace
                || (mousePosition.X - _dragablzItemsControl.ActualWidth) > InterTabController.HorizontalPopoutGrace)
                breachOrientation = Orientation.Horizontal;
            else if (mousePosition.Y < -InterTabController.VerticalPopoutGrace
                     || (mousePosition.Y - _dragablzItemsControl.ActualHeight) > InterTabController.VerticalPopoutGrace)
                breachOrientation = Orientation.Vertical;

            if (breachOrientation.HasValue)
            {
                var newTabHost = InterTabController.InterTabClient.GetNewHost(InterTabController.InterTabClient,
                    InterTabController.Partition, this);
                if (newTabHost == null || newTabHost.TabablzControl == null || newTabHost.Container == null)
                    throw new ApplicationException("New tab host was not correctly provided");

                var item = _dragablzItemsControl.ItemContainerGenerator.ItemFromContainer(e.DragablzItem);

                var myWindow = Window.GetWindow(this);
                if (myWindow == null) throw new ApplicationException("Unable to find owning window.");
                newTabHost.Container.Width = myWindow.RestoreBounds.Width;
                newTabHost.Container.Height = myWindow.RestoreBounds.Height;

                var dragStartWindowOffset = e.DragablzItem.TranslatePoint(new Point(), myWindow);
                dragStartWindowOffset.Offset(e.DragablzItem.MouseAtDragStart.X, e.DragablzItem.MouseAtDragStart.Y);
                var borderVector = myWindow.PointToScreen(new Point()) - new Point(myWindow.Left, myWindow.Top);
                dragStartWindowOffset.Offset(borderVector.X, borderVector.Y);

                var dragableItemHeaderPoint = e.DragablzItem.TranslatePoint(new Point(), _dragablzItemsControl);
                var dragableItemSize = new Size(e.DragablzItem.ActualWidth, e.DragablzItem.ActualHeight);
                var floatingItemSnapShots = this.VisualTreeDepthFirstTraversal()
                    .OfType<Layout>()
                    .SelectMany(l => l.FloatingDragablzItems().Select(FloatingItemSnapShot.Take))
                    .ToList();

                var interTabTransfer = new InterTabTransfer(item, e.DragablzItem, breachOrientation.Value, dragStartWindowOffset, e.DragablzItem.MouseAtDragStart, dragableItemHeaderPoint, dragableItemSize, floatingItemSnapShots);

                newTabHost.Container.Left = myWindow.Left;
                newTabHost.Container.Top = myWindow.Top;
                newTabHost.Container.Show();                
                var contentPresenter = FindChildContentPresenter(item);
                RemoveFromSource(item);
                _itemsHolder.Children.Remove(contentPresenter);                
                if (Items.Count == 0)
                    Layout.ConsolidateBranch(this);

                if (_previousSelection != null && Items.Contains(_previousSelection))
                    SelectedItem = _previousSelection;
                else
                    SelectedItem = Items.OfType<object>().FirstOrDefault();

                foreach (var dragablzItem in _dragablzItemsControl.DragablzItems())
                {
                    dragablzItem.IsDragging = false;
                    dragablzItem.IsSiblingDragging = false;
                }

                newTabHost.TabablzControl.ReceiveDrag(interTabTransfer);
                interTabTransfer.OriginatorContainer.IsDropTargetFound = true;
                e.Cancel = true;                
            }
        }

        private InterTabTransfer _interTabTransfer;

        internal void ReceiveDrag(InterTabTransfer interTabTransfer)
        {
            var myWindow = Window.GetWindow(this);
            if (myWindow == null) throw new ApplicationException("Unable to find owning window.");
            myWindow.Activate();

            _interTabTransfer = interTabTransfer;

            if (Items.Count == 0)
            {
                _dragablzItemsControl.LockedMeasure = new Size(
                    interTabTransfer.ItemPositionWithinHeader.X + interTabTransfer.ItemSize.Width,
                    interTabTransfer.ItemPositionWithinHeader.Y + interTabTransfer.ItemSize.Height);
            }

            AddToSource(interTabTransfer.Item);
            SelectedItem = interTabTransfer.Item;
            
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var layouts = this.VisualTreeDepthFirstTraversal().OfType<Layout>().ToList();

                foreach (var floatingDragablzItem in layouts.SelectMany(l => l.FloatingDragablzItems()))
                {
                    var floatingItemSnapShot = interTabTransfer.FloatingItemSnapShots.FirstOrDefault(
                        ss => ss.Content == floatingDragablzItem.Content);
                    if (floatingItemSnapShot != null)
                        floatingItemSnapShot.Apply(floatingDragablzItem);                        
                }                
            }), DispatcherPriority.Loaded);

            _dragablzItemsControl.InstigateDrag(interTabTransfer.Item, newContainer =>
            {
                newContainer.PartitionAtDragStart = interTabTransfer.OriginatorContainer.PartitionAtDragStart;
                newContainer.IsDropTargetFound = true;

                if (interTabTransfer.TransferReason == InterTabTransferReason.Breach)
                {                    
                    if (interTabTransfer.BreachOrientation == Orientation.Horizontal)
                        newContainer.Y = interTabTransfer.OriginatorContainer.Y;
                    else
                        newContainer.X = interTabTransfer.OriginatorContainer.X;
                }
                else
                {                    
                    var mouseXOnItemsControl = Native.GetCursorPos().X - _dragablzItemsControl.PointToScreen(new Point()).X;
                    newContainer.X = mouseXOnItemsControl - interTabTransfer.DragStartItemOffset.X;                    
                    newContainer.Y = 0;                    
                }
                newContainer.MouseAtDragStart = interTabTransfer.DragStartItemOffset;
            });
        }

        internal void AddToSource(object item)
        {                    
            var manualInterTabClient = InterTabController.InterTabClient as IManualInterTabClient;
            if (manualInterTabClient != null)
            {
                manualInterTabClient.Add(item);
            }
            else
            {
                CollectionTeaser collectionTeaser;
                if (CollectionTeaser.TryCreate(ItemsSource, out collectionTeaser))
                    collectionTeaser.Add(item);
                else
                    Items.Add(item);
            }            
        }        

        private void RemoveFromSource(object item)
        {
            var manualInterTabClient = InterTabController.InterTabClient as IManualInterTabClient;
            if (manualInterTabClient != null)
            {
                manualInterTabClient.Remove(item);
            }
            else
            {
                CollectionTeaser collectionTeaser;
                if (CollectionTeaser.TryCreate(ItemsSource, out collectionTeaser))
                    collectionTeaser.Remove(item);
                else
                    Items.Remove(item);
            }
        }

        /// <summary>
        /// generate a ContentPresenter for the selected item
        /// </summary>
        private void UpdateSelectedItem()        
        {            
            if (_itemsHolder == null)
            {
                return;
            }
            
            CreateChildContentPresenter(SelectedItem);            

            // show the right child
            var selectedContent = GetContent(SelectedItem);
            foreach (ContentPresenter child in _itemsHolder.Children)
            {
                child.Visibility = child.Content == selectedContent ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static object GetContent(object item)
        {
            return (item is TabItem) ? (item as TabItem).Content : item;
        }

        /// <summary>
        /// create the child ContentPresenter for the given item (could be data or a TabItem)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private void CreateChildContentPresenter(object item)
        {
            if (item == null) return;            

            var cp = FindChildContentPresenter(item);
            if (cp != null) return;

            // the actual child to be added.  cp.Tag is a reference to the TabItem
            cp = new ContentPresenter
            {
                Content = GetContent(item),
                ContentTemplate = ContentTemplate,
                ContentTemplateSelector = ContentTemplateSelector,
                ContentStringFormat = ContentStringFormat,
                Visibility = Visibility.Collapsed,                
            };
            _itemsHolder.Children.Add(cp);         
        }

        /// <summary>
        /// Find the CP for the given object.  data could be a TabItem or a piece of data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private ContentPresenter FindChildContentPresenter(object data)
        {
            if (data is TabItem)
                data = (data as TabItem).Content;

            if (data == null)
                return null;

            if (_itemsHolder == null)
                return null;

            return _itemsHolder.Children.Cast<ContentPresenter>().FirstOrDefault(cp => cp.Content == data);
        }

        private void ItemContainerGeneratorOnStatusChanged(object sender, EventArgs eventArgs)
        {
            MarkInitialSelection();
        }

        private void CloseItemHandler(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            var dragablzItem = executedRoutedEventArgs.Parameter as DragablzItem;
            if (dragablzItem == null) throw new ApplicationException("Parameter must be a DragablzItem");

            var cancel = false;
            if (ClosingItemCallback != null)
            {
                var callbackArgs = new ClosingItemCallbackArgs(Window.GetWindow(this), this, dragablzItem);
                ClosingItemCallback(callbackArgs);
                cancel = callbackArgs.IsCancelled;
            }

            if (!cancel)
                RemoveItem(dragablzItem);            
        }

        private void AddItemHandler(object sender, ExecutedRoutedEventArgs e)
        {            
            if (NewItemFactory == null)
                throw new InvalidOperationException("NewItemFactory must be provided.");

            var newItem = NewItemFactory();
            if (newItem == null) throw new ApplicationException("NewItemFactory returned null.");

            AddToSource(newItem);
            SelectedItem = newItem;
        }
    }
}
