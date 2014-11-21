#if NET40
using System.Collections;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dragablz.Core
{
    internal static class Extensions
    {
        public static IEnumerable<TContainer> Containers<TContainer>(this ItemsControl itemsControl) where TContainer : class
        {
#if NET45
            for (var i = 0; i < itemsControl.ItemContainerGenerator.Items.Count; i++)
#endif
#if NET40
            var propertyInfo = typeof (ItemContainerGenerator).GetProperty("Items", BindingFlags.NonPublic);
            var list = (IList) propertyInfo.GetValue(itemsControl, new object[0]);
            for (var i = 0; i < list.Count; i++)
#endif
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

        public static IEnumerable<object> VisualTreeDepthFirstTraversal(this DependencyObject node)
        {
            if (node == null) yield break;
            yield return node;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
            {
                var child = VisualTreeHelper.GetChild(node, i);
                foreach (var d in child.VisualTreeDepthFirstTraversal())
                {
                    yield return d;
                }
            }
        }
    }
}