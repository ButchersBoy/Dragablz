using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Dragablz.Themes
{
    public partial class TabWindow : ResourceDictionary
    {
        public TabWindow()
        {
            InitializeComponent();
        }
        private void AlwaysOnTop(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.Topmost = !window.Topmost;
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            if (window.WindowState == WindowState.Normal)
            {
                window.WindowState = WindowState.Maximized;
            }
            else
            {
                window.WindowState = WindowState.Normal;
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.Close();
        }

        private void Border_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                MaximizeWindow(sender, e);
            
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            window.DragMove();
        }

		#region ResizeWindows
		bool ResizeInProcess = false;
		private void Resize_Init(object sender, MouseButtonEventArgs e)
		{
			var window = (Window)((FrameworkElement)sender).TemplatedParent;

			if (window.WindowState == WindowState.Maximized)
				return;
            
			if (sender is Rectangle rectangle)
			{
				ResizeInProcess = true;
				rectangle.CaptureMouse();
			}
		}

		private void Resize_End(object sender, MouseButtonEventArgs e)
		{
			if (sender is Rectangle rectangle)
			{
				ResizeInProcess = false; ;
				rectangle.ReleaseMouseCapture();
			}
		}

		private void Resizeing_Form(object sender, MouseEventArgs e)
		{
			if (ResizeInProcess)
			{
				Rectangle senderRect = sender as Rectangle;
				Window mainWindow = senderRect.Tag as Window;
				if (senderRect != null)
				{
					double width = e.GetPosition(mainWindow).X;
					double height = e.GetPosition(mainWindow).Y;
					senderRect.CaptureMouse();
					if (senderRect.Name.ToLower().Contains("right"))
					{
						width += 5;
						if (width > 0)
							mainWindow.Width = width;
					}
					if (senderRect.Name.ToLower().Contains("left"))
					{
						width -= 5;
						mainWindow.Left += width;
						width = mainWindow.Width - width;
						if (width > 0)
						{
							mainWindow.Width = width;
						}
					}
					if (senderRect.Name.ToLower().Contains("bottom"))
					{
						height += 5;
						if (height > 0)
							mainWindow.Height = height;
					}
					if (senderRect.Name.ToLower().Contains("top"))
					{
						height -= 5;
						mainWindow.Top += height;
						height = mainWindow.Height - height;
						if (height > 0)
						{
							mainWindow.Height = height;
						}
					}
				}
			}
		}
		#endregion

	}
}
