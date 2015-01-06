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
                new SimpleViewModel { Name = "Mon", SimpleContent = "Monday's child is fair of face" },
                new SimpleViewModel { Name = "Tues", SimpleContent = "Tuesday's child is full of grace" },
                new SimpleViewModel { Name = "Wed", SimpleContent = "Wednesday's child is full of woe" },
                new SimpleViewModel { Name = "Thu", SimpleContent = "Thursday's child has far to go" },
                new SimpleViewModel { Name = "Fri", SimpleContent = "Friday's child loving and giving" }//,
                //new SimpleViewModel { Name = "Sat", SimpleContent = "Saturday's child works hard for a living" },
                //new SimpleViewModel { Name = "Sun", SimpleContent = "Sunday's child is awkwardly not fitting into this demo" }                 
                );
            boundExampleModel.ToolItems.Add(
                new SimpleViewModel { Name = "January", SimpleContent = "Welcome to the January tool/float item." });
            boundExampleModel.ToolItems.Add(
                new SimpleViewModel { Name = "July", SimpleContent = "Welcome to the July tool/float item." });
                       
            new BoundExampleWindow()
            {
                DataContext = boundExampleModel
            }.Show();
            
            new QuickStartWindow().Show();
            
            app.Run();
        }
    }

    
}
