using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Dragablz;

namespace DragablzDemo
{
    public class BoundExampleInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            var view = new BoundExampleWindow();
            var model = new BoundExampleModel();            
            view.DataContext = model;            
            return new NewTabHost<Window>(view, view.InitialTabablzControl);            
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }
    }
}