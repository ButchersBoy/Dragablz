using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Dragablz.Core
{
    internal static class Extensions
    {
        public static IEnumerable<TContainer> Containers<TContainer>(this ItemsControl itemsControl) where TContainer : class
        {
            for (var i = 0; i < itemsControl.ItemContainerGenerator.Items.Count; i++)
            {
                var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as TContainer;
                if (container != null)
                    yield return container;
            }            
        }

        public static IEnumerable<TObject> Except<TObject>(this IEnumerable<TObject> first, params TObject[] second)
        {
            return first.Except((IEnumerable<TObject>)second);
        }

        public static IEnumerable<object> LogicalTreeDepthFirstTraversal(this DependencyObject node)
        {
            if (node == null) yield break;
            yield return node;

            foreach (var child in LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>()
                .SelectMany(depObj => depObj.LogicalTreeDepthFirstTraversal()))            
                yield return child;
        }

    }
}