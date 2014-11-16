using Dragablz;

namespace DragablzDemo
{
    public class BasicExampleTemplateModel
    {
        private readonly IInterTabClient _interTabClient;
        private readonly object _partition;

        public BasicExampleTemplateModel(IInterTabClient interTabClient, object partition)
        {
            _interTabClient = interTabClient;
            _partition = partition;            
        }

        public IInterTabClient InterTabClient
        {
            get { return _interTabClient; }
        }

        public object Partition
        {
            get { return _partition; }
        }
        
    }
}