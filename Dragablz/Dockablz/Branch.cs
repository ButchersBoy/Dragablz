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
    [TemplatePart(Name = FirstContentPresenterPartName, Type=typeof(ContentPresenter))]
    [TemplatePart(Name = SecondContentPresenterPartName, Type = typeof(ContentPresenter))]
    public class Branch : Control
    {
        private const string FirstContentPresenterPartName = "PART_FirstContentPresenter";
        private const string SecondContentPresenterPartName = "PART_SecondContentPresenter";

        static Branch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Branch), new FrameworkPropertyMetadata(typeof(Branch)));
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof (Orientation), typeof (Branch), new PropertyMetadata(default(Orientation)));

        public Orientation Orientation
        {
            get { return (Orientation) GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty FirstItemProperty = DependencyProperty.Register(
            "FirstItem", typeof(object), typeof(Branch), new PropertyMetadata(default(object)));

        public object FirstItem
        {
            get { return GetValue(FirstItemProperty); }
            set { SetValue(FirstItemProperty, value); }
        }

        public static readonly DependencyProperty SecondItemProperty = DependencyProperty.Register(
            "SecondItem", typeof(object), typeof(Branch), new PropertyMetadata(default(object)));

        public object SecondItem
        {
            get { return GetValue(SecondItemProperty); }
            set { SetValue(SecondItemProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            FirstContentPresenter = GetTemplateChild(FirstContentPresenterPartName) as ContentPresenter;
            SecondContentPresenter = GetTemplateChild(SecondContentPresenterPartName) as ContentPresenter;
        }

        internal ContentPresenter FirstContentPresenter { get; private set; }
        internal ContentPresenter SecondContentPresenter { get; private set; }
    }
}
