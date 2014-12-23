﻿using System;
using System.Dynamic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Dragablz.Core;
using Dragablz.Referenceless;

namespace Dragablz
{
    public enum SizeGrip
    {
        NotApplicable,
        Left,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft
    }

    [TemplatePart(Name = ThumbPartName, Type = typeof(Thumb))]
    public class DragablzItem : ContentControl
    {
        public const string ThumbPartName = "PART_Thumb";

        private IDisposable _templateSubscriptions;

        private bool _seizeDragWithTemplate;
        private Action<DragablzItem> _dragSeizedContinuation;

        static DragablzItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragablzItem), new FrameworkPropertyMetadata(typeof(DragablzItem)));            
        }
        
        public static readonly DependencyProperty XProperty = DependencyProperty.Register(
            "X", typeof (double), typeof (DragablzItem), new PropertyMetadata(default(double), OnXChanged));

        public double X
        {
            get { return (double) GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public static readonly RoutedEvent XChangedEvent =
            EventManager.RegisterRoutedEvent(
                "XChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<double>),
                typeof(DragablzItem));

        public event RoutedPropertyChangedEventHandler<double> XChanged
        {
            add { AddHandler(XChangedEvent, value); }
            remove { RemoveHandler(IsDraggingChangedEvent, value); }
        }

        private static void OnXChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {   
            var instance = (DragablzItem)d;
            var args = new RoutedPropertyChangedEventArgs<double>(
                (double)e.OldValue,
                (double)e.NewValue)
            {
                RoutedEvent = XChangedEvent
            };
            instance.RaiseEvent(args);            
        } 

        public static readonly DependencyProperty YProperty = DependencyProperty.Register(
            "Y", typeof (double), typeof (DragablzItem), new PropertyMetadata(default(double), OnYChanged));

        public double Y
        {
            get { return (double) GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        public static readonly RoutedEvent YChangedEvent =
            EventManager.RegisterRoutedEvent(
                "YChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<double>),
                typeof(DragablzItem));

        public event RoutedPropertyChangedEventHandler<double> YChanged
        {
            add { AddHandler(YChangedEvent, value); }
            remove { RemoveHandler(IsDraggingChangedEvent, value); }
        }

        private static void OnYChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (DragablzItem)d;
            var args = new RoutedPropertyChangedEventArgs<double>(
                (double)e.OldValue,
                (double)e.NewValue)
            {
                RoutedEvent = YChangedEvent
            };
            instance.RaiseEvent(args);
        }

        public static readonly DependencyProperty SizeGripProperty = DependencyProperty.RegisterAttached(
            "SizeGrip", typeof (SizeGrip), typeof (DragablzItem), new PropertyMetadata(default(SizeGrip), SizeGripPropertyChangedCallback));

        private static void SizeGripPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var thumb = (dependencyObject as Thumb);
            if (thumb == null) return;
            thumb.DragDelta += SizeThumbOnDragDelta;
        }

        private static void SizeThumbOnDragDelta(object sender, DragDeltaEventArgs dragDeltaEventArgs)
        {
            var thumb = ((Thumb) sender);
            var dragablzItem = thumb.VisualTreeAncestory().OfType<DragablzItem>().FirstOrDefault();
            if (dragablzItem == null) return;

            var sizeGrip = (SizeGrip) thumb.GetValue(SizeGripProperty);
            var width = dragablzItem.ActualWidth;
            var height = dragablzItem.ActualHeight;
            var x = dragablzItem.X;
            var y = dragablzItem.Y;
            switch (sizeGrip)
            {                                   
                case SizeGrip.NotApplicable:
                    break;
                case SizeGrip.Left:
                    width += -dragDeltaEventArgs.HorizontalChange;
                    x += dragDeltaEventArgs.HorizontalChange;
                    break;
                case SizeGrip.TopLeft:
                    width += -dragDeltaEventArgs.HorizontalChange;
                    height += -dragDeltaEventArgs.VerticalChange;
                    x += dragDeltaEventArgs.HorizontalChange;
                    y += dragDeltaEventArgs.VerticalChange;
                    break;
                case SizeGrip.Top:
                    height += -dragDeltaEventArgs.VerticalChange;                    
                    y += dragDeltaEventArgs.VerticalChange;
                    break;
                case SizeGrip.TopRight:
                    height += -dragDeltaEventArgs.VerticalChange;
                    width += dragDeltaEventArgs.HorizontalChange;
                    y += dragDeltaEventArgs.VerticalChange;
                    break;
                case SizeGrip.Right:
                    width += dragDeltaEventArgs.HorizontalChange;
                    break;
                case SizeGrip.BottomRight:
                    width += dragDeltaEventArgs.HorizontalChange;
                    height += dragDeltaEventArgs.VerticalChange;
                    break;
                case SizeGrip.Bottom:
                    height += dragDeltaEventArgs.VerticalChange;
                    break;
                case SizeGrip.BottomLeft:
                    height += dragDeltaEventArgs.VerticalChange;
                    width += -dragDeltaEventArgs.HorizontalChange;
                    x += dragDeltaEventArgs.HorizontalChange;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            dragablzItem.SetCurrentValue(XProperty, x);
            dragablzItem.SetCurrentValue(YProperty, y);
            dragablzItem.SetCurrentValue(WidthProperty, Math.Max(width, thumb.DesiredSize.Width));
            dragablzItem.SetCurrentValue(HeightProperty, Math.Max(height, thumb.DesiredSize.Height));
        }

        public static void SetSizeGrip(DependencyObject element, SizeGrip value)
        {
            element.SetValue(SizeGripProperty, value);
        }

        public static SizeGrip GetSizeGrip(DependencyObject element)
        {
            return (SizeGrip) element.GetValue(SizeGripProperty);
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(DragablzItem), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public bool IsSelected
        {
            get { return (bool) GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        private static readonly DependencyPropertyKey IsDraggingPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsDragging", typeof (bool), typeof (DragablzItem),
                new PropertyMetadata(default(bool), OnIsDraggingChanged));

        public static readonly DependencyProperty IsDraggingProperty =
            IsDraggingPropertyKey.DependencyProperty;

        public bool IsDragging
        {
            get { return (bool) GetValue(IsDraggingProperty); }
            internal set { SetValue(IsDraggingPropertyKey, value); }
        }

        public static readonly RoutedEvent IsDraggingChangedEvent =
            EventManager.RegisterRoutedEvent(
                "IsDraggingChanged",
                RoutingStrategy.Bubble,
                typeof (RoutedPropertyChangedEventHandler<bool>),
                typeof (DragablzItem));

        public event RoutedPropertyChangedEventHandler<bool> IsDraggingChanged
        {
            add { AddHandler(IsDraggingChangedEvent, value); }
            remove { RemoveHandler(IsDraggingChangedEvent, value); }
        }

        private static readonly DependencyPropertyKey IsSiblingDraggingPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsSiblingDragging", typeof (bool), typeof (DragablzItem),
                new PropertyMetadata(default(bool), OnIsSiblingDraggingChanged));

        public static readonly DependencyProperty IsSiblingDraggingProperty =
            IsSiblingDraggingPropertyKey.DependencyProperty;

        public bool IsSiblingDragging
        {
            get { return (bool) GetValue(IsSiblingDraggingProperty); }
            internal set { SetValue(IsSiblingDraggingPropertyKey, value); }
        }

        public static readonly RoutedEvent IsSiblingDraggingChangedEvent =
            EventManager.RegisterRoutedEvent(
                "IsSiblingDraggingChanged",
                RoutingStrategy.Bubble,
                typeof (RoutedPropertyChangedEventHandler<bool>),
                typeof (DragablzItem));

        public event RoutedPropertyChangedEventHandler<bool> IsSiblingDraggingChanged
        {
            add { AddHandler(IsSiblingDraggingChangedEvent, value); }
            remove { RemoveHandler(IsSiblingDraggingChangedEvent, value); }
        }

        private static void OnIsSiblingDraggingChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (DragablzItem) d;
            var args = new RoutedPropertyChangedEventArgs<bool>(
                (bool) e.OldValue,
                (bool) e.NewValue)
            {
                RoutedEvent = IsSiblingDraggingChangedEvent
            };
            instance.RaiseEvent(args);
        } 

        private static void OnIsDraggingChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (DragablzItem)d;
            var args = new RoutedPropertyChangedEventArgs<bool>(
                (bool) e.OldValue,
                (bool) e.NewValue) {RoutedEvent = IsDraggingChangedEvent};
            instance.RaiseEvent(args);            
        }

        public static readonly RoutedEvent DragStarted =
            EventManager.RegisterRoutedEvent(
                "DragStarted",
                RoutingStrategy.Bubble,
                typeof(DragablzDragStartedEventHandler),
                typeof(DragablzItem));

        protected void OnDragStarted(DragablzDragStartedEventArgs e)
        {            
            RaiseEvent(e);
        }

        public static readonly RoutedEvent DragDelta =
            EventManager.RegisterRoutedEvent(
                "DragDelta",
                RoutingStrategy.Bubble,
                typeof (DragablzDragDeltaEventHandler),
                typeof (DragablzItem));

        protected void OnDragDelta(DragablzDragDeltaEventArgs e)
        {            
            RaiseEvent(e);            
        }

        public static readonly RoutedEvent PreviewDragDelta =
            EventManager.RegisterRoutedEvent(
                "PreviewDragDelta",
                RoutingStrategy.Tunnel,
                typeof(DragablzDragDeltaEventHandler),
                typeof(DragablzItem));

        protected void OnPreviewDragDelta(DragablzDragDeltaEventArgs e)
        {            
            RaiseEvent(e);
        }

        public static readonly RoutedEvent DragCompleted =
            EventManager.RegisterRoutedEvent(
                "DragCompleted",
                RoutingStrategy.Bubble,
                typeof(DragablzDragCompletedEventHandler),
                typeof(DragablzItem));

        protected void OnDragCompleted(DragCompletedEventArgs e)
        {
            var args = new DragablzDragCompletedEventArgs(DragCompleted, this, e);
            RaiseEvent(args);
        } 
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();            

            if (_templateSubscriptions != null)
            {
                _templateSubscriptions.Dispose();
                _templateSubscriptions = null;
            }

            var thumb = GetTemplateChild(ThumbPartName) as Thumb;
            if (thumb != null)
            {
                thumb.DragStarted += ThumbOnDragStarted;
                thumb.DragDelta += ThumbOnDragDelta;
                thumb.DragCompleted += ThumbOnDragCompleted;
            }            
            
            if (_seizeDragWithTemplate && thumb != null)
            {
                if (_dragSeizedContinuation != null)
                    _dragSeizedContinuation(this);
                _dragSeizedContinuation = null;

                Dispatcher.BeginInvoke(new Action(() => thumb.RaiseEvent(new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice,
                    0,
                    MouseButton.Left) {RoutedEvent = MouseLeftButtonDownEvent})));
            }
            _seizeDragWithTemplate = false;

            _templateSubscriptions = Disposable.Create(() =>
            {
                if (thumb == null) return;
                thumb.DragStarted -= ThumbOnDragStarted;
                thumb.DragDelta -= ThumbOnDragDelta;
                thumb.DragCompleted -= ThumbOnDragCompleted;
            });
        }        

        internal void InstigateDrag(Action<DragablzItem> continuation)
        {
            _dragSeizedContinuation = continuation;
            var thumb = GetTemplateChild(ThumbPartName) as Thumb;
            if (thumb != null)
            {
                thumb.CaptureMouse();             
            }
            else
                _seizeDragWithTemplate = true;
        }

        internal Point MouseAtDragStart { get; set; }

        internal string PartitionAtDragStart { get; set; }

        internal bool IsDropTargetFound { get; set; }

        private void ThumbOnDragCompleted(object sender, DragCompletedEventArgs dragCompletedEventArgs)
        {
            OnDragCompleted(dragCompletedEventArgs);
            MouseAtDragStart = new Point();
        }        

        private void ThumbOnDragDelta(object sender, DragDeltaEventArgs dragDeltaEventArgs)
        {
            var thumb = (Thumb) sender;

            var previewEventArgs = new DragablzDragDeltaEventArgs(PreviewDragDelta, this, dragDeltaEventArgs);
            OnPreviewDragDelta(previewEventArgs);            
            if (previewEventArgs.Cancel)
                thumb.CancelDrag();
            if (!previewEventArgs.Handled)
            {
                var eventArgs = new DragablzDragDeltaEventArgs(DragDelta, this, dragDeltaEventArgs);
                OnDragDelta(eventArgs);
                if (eventArgs.Cancel)
                    thumb.CancelDrag();
            }
        }
        
        private void ThumbOnDragStarted(object sender, DragStartedEventArgs dragStartedEventArgs)
        {
            MouseAtDragStart = Mouse.GetPosition(this);
            OnDragStarted(new DragablzDragStartedEventArgs(DragStarted, this, dragStartedEventArgs));            
        }
    }
}