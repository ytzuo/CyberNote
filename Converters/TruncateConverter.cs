using System;
using System.Globalization;
using System.Windows.Data;

namespace CyberNote.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class TruncateConverter : IValueConverter
    {
        public int MaxLength { get; set; } = 7;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrEmpty(text)) return text;

            int length = parameter is int intParam ? intParam : MaxLength;
            return text.Length > length ? text.Substring(0, length) + "..." : text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}