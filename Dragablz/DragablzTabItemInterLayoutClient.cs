using System;
using System.Runtime.ConstrainedExecution;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dragablz
{
    public class DragablzTabItemInterLayoutClient :DefaultInterLayoutClient
    {
        public override INewTabHost<UIElement> GetNewHost(object partition, TabablzControl source)
        {
            var tabablzControl = new TabablzControl {DataContext = source.DataContext};

            Clone(source, tabablzControl);

            if (source.InterTabController == null)
                throw new InvalidOperationException("Source tab does not have an InterTabCOntroller set.  Ensure this is set on initial, and subsequently generated tab controls.");
            tabablzControl.SetCurrentValue(TabablzControl.NameProperty,  $"T{tabablzControl.GetHashCode()}");
            var newInterTabController = new InterTabController
            {
                InterTabClient = new DragablzTabItemInterTabClient(),
                Partition = source.InterTabController.Partition
            };
            Clone(source.InterTabController, newInterTabController);
            tabablzControl.SetCurrentValue(TabablzControl.InterTabControllerProperty, newInterTabController);            

            return new NewTabHost<UIElement>(tabablzControl, tabablzControl);
        }
    }
}