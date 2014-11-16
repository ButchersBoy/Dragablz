using System.ComponentModel;
using System.Runtime.CompilerServices;
using DragablzDemo.Annotations;

namespace DragablzDemo
{
    public class SimpleViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        
        public string Name { get; set; }

        public object SimpleContent { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}