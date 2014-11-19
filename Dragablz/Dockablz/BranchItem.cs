using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dragablz.Dockablz
{
    public class BranchItem : Control
    {
        static BranchItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BranchItem), new FrameworkPropertyMetadata(typeof(BranchItem)));
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof (Orientation), typeof (BranchItem), new PropertyMetadata(default(Orientation)));

        public Orientation Orientation
        {
            get { return (Orientation) GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty FirstItemProperty = DependencyProperty.Register(
            "FirstItem", typeof(object), typeof(BranchItem), new PropertyMetadata(default(object)));

        public object FirstItem
        {
            get { return (object)GetValue(FirstItemProperty); }
            set { SetValue(FirstItemProperty, value); }
        }

        public static readonly DependencyProperty SecondItemProperty = DependencyProperty.Register(
            "SecondItem", typeof(object), typeof(BranchItem), new PropertyMetadata(default(object)));

        public object SecondItem
        {
            get { return (object)GetValue(SecondItemProperty); }
            set { SetValue(SecondItemProperty, value); }
        }
    }


}
