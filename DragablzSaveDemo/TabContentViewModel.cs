namespace DragablzSaveDemo
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Controls;
    using System.Windows.Documents;

    /// <summary>
    /// A tab content
    /// </summary>
    public class TabContentViewModel: INotifyPropertyChanged
    {
        /// <summary>
        /// The header
        /// </summary>
        private string _header;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="model">The model to be displayed on the tab</param>
        public TabContentViewModel(TabContentModel model)
        {
            this._header = model.Content;

            // I personally don't find it great to put a control in the ViewModel. Maybe we should use a converter or a datatemplate for that?
            this.Control = new TextBlock(new Run(model.Content));
        }

        /// <summary>
        /// The tab header
        /// </summary>
        public string Header
        {
            get
            {
                return this._header;
            }

            set
            {
                if (this._header != value)
                {
                    this._header = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The control that is displayed in the tab
        /// </summary>
        public TextBlock Control { get; }

        /// <summary>
        /// The event that is raised when a property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The name of the property that was changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}