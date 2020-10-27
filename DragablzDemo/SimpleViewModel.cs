using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters;
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
#if NET40
                OnPropertyChanged("IsSelected");
#else
                OnPropertyChanged();
#endif                
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
#if NET40
        protected virtual void OnPropertyChanged(string propertyName)
#else
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
#endif
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}