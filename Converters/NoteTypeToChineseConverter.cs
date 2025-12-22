using System;
using System.Globalization;
using System.Windows.Data;

namespace CyberNote.Converters
{
    public class NoteTypeToChineseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 示例：假设 value 是枚举类型 NoteType
            if (value == null) return string.Empty;
            switch (value.ToString())
            {
                case "Common": return "随手记";
                case "List": return "任务列表";
                case "RichText": return "富文本";
                default: return "其他";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 可根据需要实现反向转换
            throw new NotImplementedException();
        }
    }
}