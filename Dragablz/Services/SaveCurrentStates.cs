using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Dragablz.Services
{
    public class SaveCurrentStates
    {
        static SaveCurrentStates()
        {
            Application.Current.MainWindow.Closing += MainWindow_Closing;
        }

        private static void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var tabablzControl in FindVisualChildren<TabablzControl>(sender as Window))
            {
                SignOrder(tabablzControl);
            }

            foreach (Window item in Application.Current.Windows)
            {
                if (item != Application.Current.MainWindow)
                    item.Close();
            }

            using (var file = new FileStream("Init.json", FileMode.Create))
            {
                if (false)
                {
                    file.Close();
                    return;
                }

                var tabs = FindVisualChildren<DragablzTabItem>(sender as Window);

                var config = JsonConvert.SerializeObject(tabs.Select(t => t.CurrentState), Formatting.Indented);
                byte[] configBytes = ASCIIEncoding.ASCII.GetBytes(config);
                file.Write(configBytes, 0, configBytes.Length);
                file.Close();
            }

        }

        private static void SignOrder(TabablzControl tabablzControl)
        {
            int counter = 0;
            foreach (var item in tabablzControl.GetOrderedHeaders())
            {
                if (item.Content is DragablzTabItem dragablzTabItem)
                {
                    dragablzTabItem.SetValue(DragablzTabItem.OrderProperty, counter++);
                    if (!dragablzTabItem.IsMainWindow)
                    {
                        var window = Window.GetWindow(dragablzTabItem);
                        dragablzTabItem.SetCurrentValue(DragablzTabItem.WidthProperty, window.ActualWidth);
                        dragablzTabItem.SetCurrentValue(DragablzTabItem.HeightProperty, window.ActualHeight);
                    }
                }
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

    }
}
