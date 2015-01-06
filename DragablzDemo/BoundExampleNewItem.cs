using System;

namespace DragablzDemo
{
    public static class BoundExampleNewItem
    {
        public static Func<SimpleViewModel> Factory 
        {
            get
            {
                return
                    () =>
                    {
                        var dateTime = DateTime.Now;

                        return new SimpleViewModel()
                        {
                            Name = dateTime.ToLongTimeString(),
                            SimpleContent = dateTime.ToString("R")
                        };
                    };
            }        
        }
    }
}