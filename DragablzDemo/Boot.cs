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
                new SimpleViewModel { Name = "Mon", SimpleContent = "Monday" },
                new SimpleViewModel { Name = "Tues", SimpleContent = "Tuesday" },              
                new SimpleViewModel { Name = "Wed", SimpleContent = "Wednesday" },
                new SimpleViewModel { Name = "Thu", SimpleContent = "Thursday" },
                new SimpleViewModel { Name = "Fri", SimpleContent = "Friday" },
                new SimpleViewModel { Name = "Sat", SimpleContent = "Sunday" },
                new SimpleViewModel { Name = "Sun", SimpleContent = "Sunday" }                 
                );            
                       
            new BoundExampleWindow()
            {
                DataContext = boundExampleModel
            }.Show();

            new QuickStartWindow().Show();
            
            app.Run();
        }
    }

    
}
