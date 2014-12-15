using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Dragablz.Core;
using Dragablz.Dockablz;
using Dragablz.Referenceless;

namespace Dragablz
{
    /// <summary>
    /// It is not necessary to use a <see cref="DragablzWindow"/> to gain tab dragging features.
    /// What this Window does is allow a quick way to remove the Window border, and support transparency whilst
    /// dragging.  
    /// </summary>
    [TemplatePart(Name = "PART_WindowRestoreThumb", Type = typeof(Thumb))]
    public class DragablzWindow : Window
    {
        public const string WindowMoveThumbPartName = "PART_WindowRestoreThumb";
        private readonly SerialDisposable _templateSubscription = new SerialDisposable();

        static DragablzWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragablzWindow), new FrameworkPropertyMetadata(typeof(DragablzWindow)));            
        }

        public DragablzWindow()
        {
            AddHandler(DragablzItem.DragStarted, new DragablzDragStartedEventHandler(ItemDragStarted), true);
            AddHandler(DragablzItem.DragCompleted, new DragablzDragCompletedEventHandler(ItemDragCompleted), true);
        }

        private static readonly DependencyPropertyKey IsWindowBeingDraggedByTabPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsBeingDraggedByTab", typeof (bool), typeof (DragablzWindow),
                new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsBeingDraggedByTabProperty =
            IsWindowBeingDraggedByTabPropertyKey.DependencyProperty;

        public bool IsBeingDraggedByTab
        {
            get { return (bool) GetValue(IsBeingDraggedByTabProperty); }
            private set { SetValue(IsWindowBeingDraggedByTabPropertyKey, value); }
        }

        private void ItemDragCompleted(object sender, DragablzDragCompletedEventArgs e)
        {            
            IsBeingDraggedByTab = false;
        }

        private void ItemDragStarted(object sender, DragablzDragStartedEventArgs e)
        {
            var sourceOfDragItemsControl = ItemsControl.ItemsControlFromItemContainer(e.DragablzItem) as DragablzItemsControl;
            if (sourceOfDragItemsControl == null) return;

            var sourceTab = TabablzControl.GetOwnerOfHeaderItems(sourceOfDragItemsControl);

            if (sourceOfDragItemsControl.Items.Count != 1
                || (sourceTab.InterTabController != null && !sourceTab.InterTabController.MoveWindowWithSolitaryTabs)
                || Layout.IsContainedWithinBranch(sourceOfDragItemsControl))
                return;

            IsBeingDraggedByTab = true;
        }

        public override void OnApplyTemplate()
        {
            var windowRestoreThumb = GetTemplateChild(WindowMoveThumbPartName) as Thumb;

            _templateSubscription.Disposable = Disposable.Create(() =>
            {
                if (windowRestoreThumb == null) return;

                windowRestoreThumb.DragDelta -= WindowMoveThumbOnDragDelta;

            });

            base.OnApplyTemplate();

            if (windowRestoreThumb != null)
                windowRestoreThumb.DragDelta += WindowMoveThumbOnDragDelta;
            
            MouseLeftButtonDown += (s, e) => DragMove();            
        }

        protected IntPtr CriticalHandle
        {
            get
            {
                var value = typeof (Window).GetProperty("CriticalHandle", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(this);
                return (IntPtr) value;
            }
        }

        private void WindowMoveThumbOnDragDelta(object sender, DragDeltaEventArgs dragDeltaEventArgs)
        {
            if (WindowState != WindowState.Maximized ||
                (!(Math.Abs(dragDeltaEventArgs.HorizontalChange) > 2) &&
                 !(Math.Abs(dragDeltaEventArgs.VerticalChange) > 2))) return;

            WindowState = WindowState.Normal;
            Native.SendMessage(CriticalHandle, WindowMessage.WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);    
            Native.SendMessage(CriticalHandle, WindowMessage.WM_SYSCOMMAND, (IntPtr)Native.SC_MOUSEMOVE, IntPtr.Zero);
        }
    }
}
