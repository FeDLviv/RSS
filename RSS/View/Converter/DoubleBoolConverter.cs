using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Data;

namespace RSS.View.Converter
{
    class DoubleBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values != null && values.Length == 2)
            {
                bool first, second;
                if (bool.TryParse(values[0].ToString(), out first) && bool.TryParse(values[1].ToString(), out second))
                {
                    return (first && second);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}