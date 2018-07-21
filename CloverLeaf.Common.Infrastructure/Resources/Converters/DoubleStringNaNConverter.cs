using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CloverLeaf.Common.Infrastructure.Resources.Converters
{
    public class DoubleStringNaNConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                if (double.TryParse((string)value, out double d))
                    if (!double.IsNaN(d)) return d;
                return "-";
            }
            else if (value is int || value is double)
            {
                if (!double.IsNaN((double)value)) return (double)value;
            }
            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                if (double.TryParse((string)value, out double d))
                    if (!double.IsNaN(d)) return d;
                return "-";
            }
            else if (value is int || value is double)
            {
                if (!double.IsNaN((double)value)) return (double)value;
            }
            return "-";
        }
    }
}
