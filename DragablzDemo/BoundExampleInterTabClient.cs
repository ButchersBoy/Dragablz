using System.Windows;
using Dragablz;

namespace DragablzDemo
{
    public class BoundExampleInterTabClient : IInterTabClient
    {
        public INewTabHost GetNewHost(IInterTabClient interTabClient, object partition)
        {
            var view = new BoundExampleWindow();
            var model = new BoundExampleModel();
            view.DataContext = model;
            return new NewTabHost(view, view.TabablzControl);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.CloseWindow;
        }
    }
}