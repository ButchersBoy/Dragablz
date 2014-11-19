using System.Windows;
using System.Windows.Controls;

namespace Dragablz.Dockablz
{
    public class Layout : ContentControl
    {
        static Layout()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Layout), new FrameworkPropertyMetadata(typeof(Layout)));
        }
    }

    public enum DropZoneLocation
    {        
        Top,
        Right,
        Bottom,
        Left,     
    }

    public class DropZone : Control
    {
        static DropZone()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropZone), new FrameworkPropertyMetadata(typeof(DropZone)));
        }

        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            "Location", typeof (DropZoneLocation), typeof (DropZone), new PropertyMetadata(default(DropZoneLocation)));

        public DropZoneLocation Location
        {
            get { return (DropZoneLocation) GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }
    }
}