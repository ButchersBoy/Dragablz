using System;
using System.Linq;
using System.Windows;
using Dragablz.Core;

namespace Dragablz
{
    public class DefaultInterTabClient : IInterTabClient
    {        
        public INewTabHost GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            if (source == null) throw new ArgumentNullException("source");
            var sourceWindow = Window.GetWindow(source);
            if (sourceWindow == null) throw new ApplicationException("Unable to ascrtain source window.");
            var newWindow = (Window)Activator.CreateInstance(sourceWindow.GetType());

            var newTabablzControl = newWindow.LogicalTreeDepthFirstTraversal().OfType<TabablzControl>().FirstOrDefault();
            if (newTabablzControl == null) throw new ApplicationException("Unable to ascrtain tab control.");

            newTabablzControl.Items.Clear();

            return new NewTabHost(newWindow, newTabablzControl);            
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.CloseWindow;
        }
    }
}