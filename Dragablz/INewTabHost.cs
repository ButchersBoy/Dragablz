using System.Windows;

namespace Dragablz
{
    public interface INewTabHost
    {
        Window Window { get; }
        TabablzControl TabablzControl { get; }
    }
}