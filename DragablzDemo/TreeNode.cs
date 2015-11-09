using System.Collections.ObjectModel;

namespace DragablzDemo
{
    public class TreeNode
    {
        private readonly ObservableCollection<TreeNode> _children = new ObservableCollection<TreeNode>();

        public object Content { get; set; }

        public ObservableCollection<TreeNode> Children
        {
            get { return _children; }
        }
    }
}