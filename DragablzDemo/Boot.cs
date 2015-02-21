using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using Dragablz;

namespace DragablzDemo
{
    public class Boot
    {
        [STAThread]
        public static void Main(string[] args)
        {            
            var app = new App {ShutdownMode = ShutdownMode.OnLastWindowClose};
            app.InitializeComponent();
            
            new BasicExampleMainWindow
            {
                DataContext = new BasicExampleMainModel()
            }.Show();                                      
                
            var boundExampleModel = new BoundExampleModel(
                new HeaderedItemViewModel { Header = "Mon", Content = "Monday's child is fair of face" },
                new HeaderedItemViewModel { Header = "MDI Demo", Content = new MdiExample() },
                new HeaderedItemViewModel { Header = "Layout Info", Content = new LayoutManagementExample { DataContext = new LayoutManagementExampleViewModel()} },
                new HeaderedItemViewModel { Header = new CustomHeaderViewModel { Header = "Header"}, Content = "This tab illustrates how an individual header can be customised, without having to change the DragablzItem tab header template." },                
                new HeaderedItemViewModel { Header = "Tues", Content = "Tuesday's child is full of grace" }//,
                //new HeaderedItemViewModel { Header = "Wed", Content = "Wednesday's child is full of woe" }//,
                //new HeaderedItemViewModel { Header = "Thu", Content = "Thursday's child has far to go" },
                //new HeaderedItemViewModel { Header = "Fri", Content = "Friday's child loving and giving" }//,
                //new HeaderedItemViewModel { Header = "Sat", Content = "Saturday's child works hard for a living" },
                //new HeaderedItemViewModel { Header = "Sun", Content = "Sunday's child is awkwardly not fitting into this demo" }                 
                );
            /*
            boundExampleModel.ToolItems.Add(
                new HeaderedItemViewModel { Header = "January", Content = "Welcome to the January tool/float item." });
            boundExampleModel.ToolItems.Add(
                new HeaderedItemViewModel { Header = "July", Content = "Welcome to the July tool/float item." });
             */
                       
            new BoundExampleWindow()
            {
                DataContext = boundExampleModel
            }.Show();
            
            new QuickStartWindow().Show();
            
            app.Run();
        }
    }

    
}
