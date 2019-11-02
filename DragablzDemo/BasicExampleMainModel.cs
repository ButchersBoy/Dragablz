using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Dragablz;
using DragablzDemo.Annotations;

namespace DragablzDemo
{
    public class BasicExampleMainModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<Person> _people = new ObservableCollection<Person>();
        private readonly ObservableCollection<SimpleViewModel> _viewModels = new ObservableCollection<SimpleViewModel>();
        private readonly ICommand _addNewPerson;
        private readonly ICommand _addNewViewModel;
        private readonly IInterTabClient _interTabClient;
        private int _newPersonCount;
        private int _newViewModelCount;
        private SimpleViewModel _selectedViewModel;
        private readonly PositionMonitor _basicColourMonitor;
        private string _basicColourMonitorText = "awaiting...";
        private readonly VerticalPositionMonitor _peopleMonitor;
        private string _peopleMonitorText = "awaiting...";

        public BasicExampleMainModel()
        {
            _basicColourMonitor = new PositionMonitor();
            _basicColourMonitor.LocationChanged += (sender, args) => BasicColourMonitorText = args.Location.ToString();
            _peopleMonitor = new VerticalPositionMonitor();
            _peopleMonitor.OrderChanged += PeopleMonitorOnOrderChanged;

            _people.Add(new Person {FirstName = "Albert", LastName = "Einstein"});
            _people.Add(new Person { FirstName = "Neil", LastName = "Tyson" });
            _people.Add(new Person { FirstName = "James", LastName = "Willock" }); //i move in esteemed circles ;)

            _viewModels.Add(new SimpleViewModel { Name = "Alpha", SimpleContent = "This is the alpha content"});
            _viewModels.Add(new SimpleViewModel { Name = "Beta", SimpleContent = "Beta content", IsSelected = true });
            _viewModels.Add(new SimpleViewModel { Name = "Gamma", SimpleContent = "And here is the gamma content" });

            SelectedViewModel = _viewModels[1];

            _interTabClient = new BasicExampleInterTabClient();

            _addNewPerson = new AnotherCommandImplementation(
                x =>
                {
                    _newPersonCount++;
                    _people.Add(new Person
                    {
                        FirstName = "Hello_" + _newPersonCount,
                        LastName = "World_" + _newPersonCount
                    });
                });

            _addNewViewModel = new AnotherCommandImplementation(
                x =>
                {
                    _newViewModelCount++;
                    _viewModels.Add(new SimpleViewModel()
                    {
                        Name = "New Tab " + _newViewModelCount,
                        SimpleContent = "New Tab Content " + _newViewModelCount
                    });
                    SelectedViewModel = _viewModels.Last();
                });
        }

        public string BasicColourMonitorText
        {
            get { return _basicColourMonitorText; }
            set
            {
                if (_basicColourMonitorText == value) return;

                _basicColourMonitorText = value;
#if NET40
                OnPropertyChanged("BasicColourMonitorText");
#endif
#if NET45
                OnPropertyChanged();
#endif 
            }
        }

        public string PeopleMonitorText
        {
            get { return _peopleMonitorText; }
            set
            {
                if (_peopleMonitorText == value) return;

                _peopleMonitorText = value;
#if NET40
                OnPropertyChanged("PeopleMonitorText");
#endif
#if NET45
                OnPropertyChanged();
#endif 
            }
        }

        public ReadOnlyObservableCollection<Person> People
        {
            get { return new ReadOnlyObservableCollection<Person>(_people); }            
        }

        public ICommand AddNewPerson
        {
            get { return _addNewPerson; }
        }

        public ObservableCollection<SimpleViewModel> ViewModels
        {
            get { return _viewModels; }
        }

        public SimpleViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set
            {
                if (_selectedViewModel == value) return;
                _selectedViewModel = value;
#if NET40
                OnPropertyChanged("SelectedViewModel");
#endif
#if NET45
                OnPropertyChanged();
#endif 
            }
        }

        public ICommand AddNewViewModel
        {
            get { return _addNewViewModel; }
        }

        public PositionMonitor BasicColourMonitor
        {
            get { return _basicColourMonitor; }            
        }

        public PositionMonitor PeopleMonitor
        {
            get { return _peopleMonitor; }
        }

        public IInterTabClient BasicInterTabClient
        {
            get { return _interTabClient; }
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

        private void PeopleMonitorOnOrderChanged(object sender, OrderChangedEventArgs orderChangedEventArgs)
        {
            PeopleMonitorText = orderChangedEventArgs.NewOrder.OfType<Person>()
                .Aggregate("", (accumalate, person) => accumalate + person.LastName + ", ");
        }
    }

    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
