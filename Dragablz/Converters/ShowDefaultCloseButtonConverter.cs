using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dragablz.Converters
{
    public class ShowDefaultCloseButtonConverter : IMultiValueConverter
    {
        /// <summary>
        /// [0] is owning tabcontrol ShowDefaultCloseButton value.
        /// [1] is owning tabcontrol FixedHeaderCount value.
        /// [2] is item LogicalIndex
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool) values[0] && (int)values[2] >= (int)values[1]) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;            
        }
    }
}