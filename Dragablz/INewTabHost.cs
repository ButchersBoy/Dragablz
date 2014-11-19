using System.Windows;

namespace Dragablz
{
    public interface INewTabHost<TElement> where TElement : UIElement
    {
        TElement Container { get; }
        TabablzControl TabablzControl { get; }
    }
}