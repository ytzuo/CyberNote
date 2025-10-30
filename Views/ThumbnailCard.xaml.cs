using System;
using System.Windows;
using System.Windows.Controls;

namespace CyberNote.Views
{
    /// <summary>
    /// 缩略图卡片控件 - 符合 MVVM 的可复用控件
    /// </summary>
    public partial class ThumbnailCard : UserControl
    {
        // 依赖属性：笔记类型
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(
                nameof(Type),
                typeof(string),
                typeof(ThumbnailCard),
                new PropertyMetadata("Common")); // 默认值

        // 依赖属性：创建日期
        public static readonly DependencyProperty CreateDateProperty =
            DependencyProperty.Register(
                nameof(CreateDate),
                typeof(DateTime),
                typeof(ThumbnailCard),
                new PropertyMetadata(DateTime.Now));

        // 依赖属性：文本行0（标题）
        public static readonly DependencyProperty Text0Property =
            DependencyProperty.Register(
                nameof(Text0),
                typeof(string),
                typeof(ThumbnailCard),
                new PropertyMetadata(""));

        // 依赖属性：文本行1
        public static readonly DependencyProperty Text1Property =
            DependencyProperty.Register(
                nameof(Text1),
                typeof(string),
                typeof(ThumbnailCard),
                new PropertyMetadata(""));

        // 依赖属性：文本行2
        public static readonly DependencyProperty Text2Property =
            DependencyProperty.Register(
                nameof(Text2),
                typeof(string),
                typeof(ThumbnailCard),
                new PropertyMetadata(""));

        // 依赖属性：文本行3
        public static readonly DependencyProperty Text3Property =
            DependencyProperty.Register(
                nameof(Text3),
                typeof(string),
                typeof(ThumbnailCard),
                new PropertyMetadata(""));

        // 暴露公共属性（自动支持绑定）
        public string Type
        {
            get { return (string)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public DateTime CreateDate
        {
            get { return (DateTime)GetValue(CreateDateProperty); }
            set { SetValue(CreateDateProperty, value); }
        }

        public string Text0
        {
            get { return (string)GetValue(Text0Property); }
            set { SetValue(Text0Property, value); }
        }

        public string Text1
        {
            get { return (string)GetValue(Text1Property); }
            set { SetValue(Text1Property, value); }
        }

        public string Text2
        {
            get { return (string)GetValue(Text2Property); }
            set { SetValue(Text2Property, value); }
        }

        public string Text3
        {
            get { return (string)GetValue(Text3Property); }
            set { SetValue(Text3Property, value); }
        }

        // 静态资源，用于截断字符串（可选：也可在 ViewModel 中处理）
        public static string TruncateString(string input, int maxLength = 7)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input.Length > maxLength
                ? input.Substring(0, maxLength) + "..."
                : input;
        }

        public ThumbnailCard()
        {
            InitializeComponent();
        }
    }
}