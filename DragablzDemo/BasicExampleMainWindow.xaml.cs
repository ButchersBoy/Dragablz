using System.Windows;

namespace DragablzDemo
{
    /// <summary>
    /// Interaction logic for BasicExampleMainWindow.xaml
    /// </summary>
    public partial class BasicExampleMainWindow : Window
    {
        public BasicExampleMainWindow()
        {
            InitializeComponent();

            DataContext = new BasicExampleMainModel();            
        }
    }
}
