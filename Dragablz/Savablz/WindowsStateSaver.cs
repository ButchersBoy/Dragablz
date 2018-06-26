namespace Dragablz.Savablz
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Threading;
    using Dragablz.Dockablz;

    /// <summary>
    /// Saves/restore the state of the windows
    /// </summary>
    public static class WindowsStateSaver
    {
        /// <summary>
        /// Gets the state of all windows in the app
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model, currently displayed in the app.</typeparam>
        /// <param name="tabContentModelConverter">The converter that transforms tab view models to models</param>
        /// <returns>The state of all windows</returns>
        public static IEnumerable<LayoutWindowState<TTabModel>> GetWindowsState<TTabModel, TTabViewModel>(Func<TTabViewModel, TTabModel> tabContentModelConverter)
        {
            return Layout.GetLoadedInstances().Select(layout => GetLayoutState(layout, tabContentModelConverter));
        }

        /// <summary>
        /// Gets the state of a single window
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model, currently displayed in the app.</typeparam>
        /// <param name="layout">The layout to be inspected</param>
        /// <param name="tabContentModelConverter">The converter that transforms tab view models to models</param>
        /// <returns>The state of the specified window</returns>
        private static LayoutWindowState<TTabModel> GetLayoutState<TTabModel, TTabViewModel>(Layout layout, Func<TTabViewModel, TTabModel> tabContentModelConverter)
        {
            var window = Window.GetWindow(layout);
            if (window == null)
            {
                throw new InvalidOperationException("The layout is not bound to any window");
            }

            var layoutAccessor = layout.Query();

            BranchItemState<TTabModel> root = null;
            layoutAccessor.Visit(
                branchVisitor => root = new BranchItemState<TTabModel>(GetBranchState(branchVisitor, tabContentModelConverter), null),
                tabablzControl => root = new BranchItemState<TTabModel>(null, GetTabSetState(tabablzControl, tabContentModelConverter))
            );

            return new LayoutWindowState<TTabModel>(window.Left, window.Top, window.Width, window.Height, window.WindowState, root);
        }

        /// <summary>
        /// Gets the state of a branch
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model, currently displayed in the app.</typeparam>
        /// <param name="branchVisitor">The branch to be inspected</param>
        /// <param name="tabContentModelConverter">The converter that transforms tab view models to models</param>
        /// <returns>The read state of the branch</returns>
        private static BranchState<TTabModel> GetBranchState<TTabModel, TTabViewModel>(BranchAccessor branchVisitor, Func<TTabViewModel, TTabModel> tabContentModelConverter)
        {
            BranchItemState<TTabModel> firstState = null;
            BranchItemState<TTabModel> secondState = null;

            if (branchVisitor.FirstItemBranchAccessor != null)
            {
                firstState = new BranchItemState<TTabModel>(GetBranchState(branchVisitor.FirstItemBranchAccessor, tabContentModelConverter), null);
            }
            else
            {
                firstState = new BranchItemState<TTabModel>(null, GetTabSetState(branchVisitor.FirstItemTabablzControl, tabContentModelConverter));
            }

            if (branchVisitor.SecondItemBranchAccessor != null)
            {
                secondState = new BranchItemState<TTabModel>(GetBranchState(branchVisitor.SecondItemBranchAccessor, tabContentModelConverter), null);
            }
            else
            {
                secondState = new BranchItemState<TTabModel>(null, GetTabSetState(branchVisitor.SecondItemTabablzControl, tabContentModelConverter));
            }

            return new BranchState<TTabModel>(
                firstState,
                secondState,
                branchVisitor.Branch.Orientation,
                branchVisitor.Branch.GetFirstProportion());
        }

        /// <summary>
        /// Gets the state of a TabablzControl so that it can be serialized
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model, currently displayed in the app.</typeparam>
        /// <param name="tabablzControl">The control to be </param>
        /// <param name="tabContentModelConverter">The converter that transforms tab view models to models</param>
        /// <returns>The state of the tab set</returns>
        public static TabSetState<TTabModel> GetTabSetState<TTabModel, TTabViewModel>(TabablzControl tabablzControl, Func<TTabViewModel, TTabModel> tabContentModelConverter)
        {
            int? selectedIndex = tabablzControl.SelectedIndex;
            if (selectedIndex == -1)
            {
                selectedIndex = null;
            }

            return new TabSetState<TTabModel>(selectedIndex, tabablzControl.Items.Cast<TTabViewModel>().Select(tabContentModelConverter));
        }

        /// <summary>
        /// Restors the state of all windows
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model to be displayed in the app.</typeparam>
        /// <param name="windowInitialTabablzControl">The initial tabablz control that will be used for restore</param>
        /// <param name="layoutWindowsState">The state of the windows</param>
        /// <param name="viewModelFactory">The function that creates the view model based on a model</param>
        public static void RestoreWindowsState<TTabModel, TTabViewModel>(TabablzControl windowInitialTabablzControl, LayoutWindowState<TTabModel>[] layoutWindowsState, Func<TTabModel, TTabViewModel> viewModelFactory)
        {
            if (!layoutWindowsState.Any())
            {
                return;
            }

            var mainWindowState = layoutWindowsState[0];
            var mainWindow = Window.GetWindow(windowInitialTabablzControl);
            if (mainWindow == null)
            {
                throw new InvalidOperationException("Window not found");
            }

            mainWindow.Width = mainWindowState.Width;
            mainWindow.Height = mainWindowState.Height;
            mainWindow.Left = mainWindowState.X;
            mainWindow.Top = mainWindowState.Y;
            mainWindow.WindowState = mainWindowState.WindowState;

            RestoreBranchItemState(windowInitialTabablzControl, mainWindowState.Child, viewModelFactory);

            foreach (var windowState in layoutWindowsState.Skip(1))
            {
                var interTabController = windowInitialTabablzControl.InterTabController;
                var newHost = interTabController.InterTabClient.GetNewHost(interTabController.InterTabClient, interTabController.Partition, windowInitialTabablzControl);
                newHost.Container.Width = windowState.Width;
                newHost.Container.Height = windowState.Height;
                newHost.Container.Left = windowState.X;
                newHost.Container.Top = windowState.Y;
                newHost.Container.WindowState = windowState.WindowState;
                newHost.Container.Show();
                RestoreBranchItemState(newHost.TabablzControl, windowState.Child, viewModelFactory);
            }
        }

        /// <summary>
        /// Restores the state of the tabSet
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model to be displayed in the app.</typeparam>
        /// <param name="tabablzControl">The control in which to restore the items</param>
        /// <param name="tabSetState">The state of the tab set to be restored</param>
        /// <param name="viewModelFactory">The function that creates the view model based on a model</param>
        public static void RestoreTabSetState<TTabModel, TTabViewModel>(TabablzControl tabablzControl, TabSetState<TTabModel> tabSetState, Func<TTabModel, TTabViewModel> viewModelFactory)
        {
            foreach (var tabModel in tabSetState.TabItems)
            {
                tabablzControl.AddToSource(viewModelFactory(tabModel));
            }

            tabablzControl.Dispatcher.BeginInvoke(new Action(() =>
            {
                tabablzControl.SelectedIndex = tabSetState.SelectedTabItemIndex ?? -1;
            }), DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Restores the state of the branch
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model to be displayed in the app.</typeparam>
        /// <param name="tabablzControl">The control in which to restore the items</param>
        /// <param name="branchState">The state of the branch to be restored</param>
        /// <param name="viewModelFactory">The function that creates the view model based on a model</param>
        private static void RestoreBranchState<TTabModel, TTabViewModel>(TabablzControl tabablzControl, BranchState<TTabModel> branchState, Func<TTabModel, TTabViewModel> viewModelFactory)
        {
            var branchResult = Layout.Branch(tabablzControl, CopyTabablzControl(tabablzControl), branchState.Orientation, false, branchState.Ratio);
            RestoreBranchItemState(tabablzControl, branchState.FirstChild, viewModelFactory);
            RestoreBranchItemState(branchResult.TabablzControl, branchState.SecondChild, viewModelFactory);
        }

        /// <summary>
        /// Restores the state of a branch item
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model to be displayed in the app.</typeparam>
        /// <param name="tabablzControl">The control in which to restore the items</param>
        /// <param name="branchItemState">The state of the branch item to be restored</param>
        /// <param name="viewModelFactory">The function that creates the view model based on a model</param>
        private static void RestoreBranchItemState<TTabModel, TTabViewModel>(TabablzControl tabablzControl, BranchItemState<TTabModel> branchItemState, Func<TTabModel, TTabViewModel> viewModelFactory)
        {
            if (branchItemState.ItemAsTabSet != null)
            {
                RestoreTabSetState(tabablzControl, branchItemState.ItemAsTabSet, viewModelFactory);
            }
            else if(branchItemState.ItemAsBranch != null)
            {
                RestoreBranchState(tabablzControl, branchItemState.ItemAsBranch, viewModelFactory);
            }
        }

        /// <summary>
        /// Creates a new TabablzControl based on the specified control
        /// </summary>
        /// <param name="tabablzControl">The control to copy</param>
        /// <returns>The created control</returns>
        private static TabablzControl CopyTabablzControl(TabablzControl tabablzControl)
        {
            var result = new TabablzControl
            {
                InterTabController = new InterTabController
                {
                    InterTabClient = tabablzControl.InterTabController.InterTabClient,
                    Partition = tabablzControl.InterTabController.Partition
                }
            };
            return result;
        }
    }
}