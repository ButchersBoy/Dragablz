namespace DragablzSaveDemo
{
    /// <summary>
    /// The serializable model that stores the state of the tab
    /// </summary>
    public class TabContentModel
    {
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="content">The tab content</param>
        public TabContentModel(string content)
        {
            this.Content = content;
        }

        /// <summary>
        /// The tab content
        /// </summary>
        public string Content {get;}
    }
}