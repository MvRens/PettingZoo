using System;
using System.Globalization;
using System.Windows.Data;

namespace PettingZoo.WPF.ValueConverters
{
    public class SameReferenceConverter : IMultiValueConverter
    {
        public object Convert(object?[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return ReferenceEquals(values[0], values[1]);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
