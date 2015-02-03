using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using Dragablz;
using Dragablz.Dockablz;
using DragablzDemo.Annotations;

namespace DragablzDemo
{
    public class BoundExampleModel
    {
        private readonly IInterTabClient _interTabClient = new BoundExampleInterTabClient();        
        private readonly ObservableCollection<SimpleViewModel> _items;
        private readonly ObservableCollection<SimpleViewModel> _toolItems = new ObservableCollection<SimpleViewModel>();

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

        public ObservableCollection<SimpleViewModel> ToolItems
        {
            get { return _toolItems; }
        }

        public ClosingTabItemCallback ClosingTabItemHandler
        {
            get { return ClosingTabItemHandlerImpl; }
        }

        /// <summary>
        /// Callback to handle tab closing.
        /// </summary>        
        private static void ClosingTabItemHandlerImpl(ClosingItemCallbackArgs<TabablzControl> args)
        {
            //in here you can dispose stuff or cancel the close

            //here's your view model:
            var simpleViewModel = args.DragablzItem.DataContext as SimpleViewModel;
            Debug.Assert(simpleViewModel != null);

            //here's how you can cancel stuff:
            //args.Cancel(); 
        }

        public ClosingFloatingItemCallback ClosingFloatingItemHandler
        {
            get { return ClosingFloatingItemHandlerImpl; }
        }

        /// <summary>
        /// Callback to handle floating toolbar/MDI window closing.
        /// </summary>        
        private static void ClosingFloatingItemHandlerImpl(ClosingItemCallbackArgs<Layout> args)
        {
            //in here you can dispose stuff or cancel the close

            //here's your view model: 
            var disposable = args.DragablzItem.DataContext as IDisposable;
            if (disposable != null)
                disposable.Dispose();

            //here's how you can cancel stuff:
            //args.Cancel(); 
        }
    }
}
