using System.Windows;

namespace Dragablz
{
    public class DefaultInterLayoutClient : IInterLayoutClient
    {
        public INewTabHost<UIElement> GetNewHost(object partition, TabablzControl source)
        {
            var tabablzControl = new TabablzControl();

            Clone(source, tabablzControl);

            var newInterTabController = new InterTabController();
            Clone(source.InterTabController, newInterTabController);
            tabablzControl.SetCurrentValue(TabablzControl.InterTabControllerProperty, newInterTabController);

            return new NewTabHost<UIElement>(tabablzControl, tabablzControl);
        }

        private static void Clone(DependencyObject from, DependencyObject to)
        {
            var localValueEnumerator = from.GetLocalValueEnumerator();
            while (localValueEnumerator.MoveNext())
            {
                if (!localValueEnumerator.Current.Property.ReadOnly
                    && !(localValueEnumerator.Current.Value is FrameworkElement))
                    to.SetCurrentValue(localValueEnumerator.Current.Property, localValueEnumerator.Current.Value);
            }
            
        }
    }
}