using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using Dragablz;
using DragablzDemo.Annotations;

namespace DragablzDemo
{
    public class BoundExampleModel
    {
        private readonly IInterTabClient _interTabClient = new BoundExampleInterTabClient();        
        private readonly ObservableCollection<SimpleViewModel> _items;

        public BoundExampleModel()
        {
            _items = new ObservableCollection<SimpleViewModel>();
        }

        public BoundExampleModel(params SimpleViewModel[] items)
        {
            _items = new ObservableCollection<SimpleViewModel>(items);
        }

        public ObservableCollection<SimpleViewModel> Items
        {
            get { return _items; }
        }

        public static Guid TabPartition
        {
            get { return new Guid("2AE89D18-F236-4D20-9605-6C03319038E6"); }
        }

        public IInterTabClient InterTabClient
        {
            get { return _interTabClient; }
        }

        public IEnumerable<HeaderAndContentModel> ToolItems
        {
            get
            {
                yield return new HeaderAndContentModel { Header = "January", Content = "Welcome to the January tool/float item."};
                yield return new HeaderAndContentModel { Header = "July", Content = "Welcome to the July tool/float item." };
            }
        }

        
    }
}
