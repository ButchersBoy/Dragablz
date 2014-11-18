using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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

        private static readonly HashSet<TabablzControl> _loadedInstances = new HashSet<TabablzControl>();

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

            Loaded += (sender, args) => _loadedInstances.Add(this);
            Unloaded += (sender, args) => _loadedInstances.Remove(this);
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

        private void MarkInitialSelection()
        {
            if (_dragablzItemsControl == null ||
                _dragablzItemsControl.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;

            if (_dragablzItemsControl != null && SelectedItem != null && !(SelectedItem is TabItem))
            {
                var containerFromItem = _dragablzItemsControl.ItemContainerGenerator.ContainerFromItem(SelectedItem) as DragablzItem;
                if (containerFromItem != null)
                    containerFromItem.IsSelected = true;
            }
        }

        private void ItemDragStarted(object sender, DragablzDragStartedEventArgs e)
        {
            //the thumb may steal the user selection, so we will try and apply it manually
            if (_dragablzItemsControl == null) return;

            var sourceOfDragItemsControl = ItemsControlFromItemContainer(e.DragablzItem) as DragablzItemsControl;
            if (sourceOfDragItemsControl != null && Equals(sourceOfDragItemsControl, _dragablzItemsControl))
            {
                var itemsControlOffset = Mouse.GetPosition(_dragablzItemsControl);
                _tabHeaderDragStartInformation = new TabHeaderDragStartInformation(e.DragablzItem, itemsControlOffset.X,
                    itemsControlOffset.Y, e.DragStartedEventArgs.HorizontalOffset, e.DragStartedEventArgs.VerticalOffset);

                foreach (var otherItem in _dragablzItemsControl.Containers<DragablzItem>().Except(e.DragablzItem))                
                    otherItem.IsSelected = false;                
                e.DragablzItem.IsSelected = true;
                var item = _dragablzItemsControl.ItemContainerGenerator.ItemFromContainer(e.DragablzItem);
                var tabItem = item as TabItem;
                if (tabItem != null)
                    tabItem.IsSelected = true;
                else
                    SelectedItem = item;
            }
        }

        private void PreviewItemDragDelta(object sender, DragablzDragDeltaEventArgs e)
        {
            if (_dragablzItemsControl == null) return;

            var sourceOfDragItemsControl = ItemsControlFromItemContainer(e.DragablzItem) as DragablzItemsControl;
            if (sourceOfDragItemsControl != null && Equals(sourceOfDragItemsControl, _dragablzItemsControl))
            {
                if (Items.Count == 1 && (InterTabController == null || InterTabController.MoveWindowWithSolitaryTabs))
                {
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
            }
        }

        private bool MonitorReentry(DragablzDragDeltaEventArgs e)
        {
            var screenMousePosition = _dragablzItemsControl.PointToScreen(Mouse.GetPosition(_dragablzItemsControl));
            
            var otherTabablzControls = _loadedInstances
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
                var item = _dragablzItemsControl.ItemContainerGenerator.ItemFromContainer(e.DragablzItem);
                
                var interTabTransfer = new InterTabTransfer(item, e.DragablzItem, Mouse.GetPosition(e.DragablzItem));
                var contentPresenter = FindChildContentPresenter(item);
                RemoveFromSource(item);
                _itemsHolder.Children.Remove(contentPresenter);

                e.DragablzItem.IsDragging = false;
                var window = Window.GetWindow(this);
                if (window != null && InterTabController.InterTabClient.TabEmptiedHandler(this, window) == TabEmptiedResponse.CloseWindow)
                    window.Close();

                target.tc.ReceiveDrag(interTabTransfer);
                e.Cancel = true;
                
                return true;
            }             
   
            return false;
        }


        private void ItemDragCompleted(object sender, DragablzDragCompletedEventArgs e)
        {
            _interTabTransfer = null;
            _dragablzItemsControl.LockedMeasure = null;
        }

        private void ItemDragDelta(object sender, DragablzDragDeltaEventArgs e)
        {
            if (_tabHeaderDragStartInformation != null &&
                Equals(_tabHeaderDragStartInformation.DragItem, e.DragablzItem) && 
                InterTabController != null)
            {
                if (InterTabController.InterTabClient == null)                    
                    throw new InvalidOperationException("An InterTabClient must be provided on an InterTabController.");
                
                MonitorBreach(e);
            }
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
                if (newTabHost == null || newTabHost.TabablzControl == null || newTabHost.Window == null)
                    throw new ApplicationException("New tab host was not correctly provided");

                var item = _dragablzItemsControl.ItemContainerGenerator.ItemFromContainer(e.DragablzItem);

                var myWindow = Window.GetWindow(this);
                if (myWindow == null) throw new ApplicationException("Unable to find owning window.");
                newTabHost.Window.Width = myWindow.RestoreBounds.Width;
                newTabHost.Window.Height = myWindow.RestoreBounds.Height;

                var dragStartWindowOffset = e.DragablzItem.TranslatePoint(new Point(), myWindow);
                dragStartWindowOffset.Offset(e.DragablzItem.MouseAtDragStart.X, e.DragablzItem.MouseAtDragStart.Y);
                var borderVector = myWindow.PointToScreen(new Point()) - new Point(myWindow.Left, myWindow.Top);
                dragStartWindowOffset.Offset(borderVector.X, borderVector.Y);

                var dragableItemHeaderPoint = e.DragablzItem.TranslatePoint(new Point(), _dragablzItemsControl);
                var dragableItemSize = new Size(e.DragablzItem.ActualWidth, e.DragablzItem.ActualHeight);

                var interTabTransfer = new InterTabTransfer(item, e.DragablzItem, breachOrientation.Value, dragStartWindowOffset, e.DragablzItem.MouseAtDragStart, dragableItemHeaderPoint, dragableItemSize);

                newTabHost.Window.Left = myWindow.Left;
                newTabHost.Window.Top = myWindow.Top;
                newTabHost.Window.Show();                
                var contentPresenter = FindChildContentPresenter(item);
                RemoveFromSource(item);
                _itemsHolder.Children.Remove(contentPresenter);                

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
            _dragablzItemsControl.InstigateDrag(interTabTransfer.Item, newContainer =>
            {
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

        private void AddToSource(object item)
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
    }
}
