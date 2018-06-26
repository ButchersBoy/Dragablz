using System.Windows;

namespace DragablzSaveDemo
{
    using Dragablz.Savablz;
    using DragablzSaveDemo.Properties;
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// This method is called on application startup
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var viewModel = new TabsViewModel();
            var window = new MainWindow
            {
                DataContext = viewModel
            };

            // A choice has been made for this app : There is a main window, and you can create other windows.
            // The app exits when the main window is closed, whether or not there are other windows open.
            // When the main window is closing, save the state into the settings
            window.Closing += this.MainWindow_Closing;

            var l = Settings.Default.Layout;

            if (string.IsNullOrWhiteSpace(l))
            {
                // Default layout and tabs
                for (var i = 0 ; i < 4 ; i++)
                {
                    viewModel.TabContents.Add(new TabContentViewModel(new TabContentModel(Guid.NewGuid().ToString())));
                }
                window.Show();
            }
            else
            {
                // Restore layout
                var windowsState = JsonConvert.DeserializeObject<LayoutWindowState<TabContentModel>[]>(l);
                window.Show();
                WindowsStateSaver.RestoreWindowsState(window.InitialTabablzControl, windowsState, m => new TabContentViewModel(m));
            }
        }

        /// <summary>
        /// Called when the main window is closing
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event args</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // Saves the layout and exit
            var windowsState =
                WindowsStateSaver.GetWindowsState<TabContentModel, TabContentViewModel>(vm =>
                    new TabContentModel(vm.Header));

            if (windowsState.First().Child == null)
            {
                // All tabs in the main window have been closed.
                // A choice have been made for this sample app : When all tabs in the main window have been closed,
                // resets the settings so that a fresh window is created next time.
                // Feel free to implement that the way you want here
                Settings.Default.Layout = null;
            }
            else
            {
                Settings.Default.Layout = JsonConvert.SerializeObject(windowsState, Formatting.None);
            }

            Settings.Default.Save();

            this.Shutdown();
        }
    }
}
