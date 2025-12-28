using System;
using System.Windows;
using System.Windows.Controls;
using CyberNote.Services;

namespace CyberNote.Views
{
    /// <summary>
    /// 缩略图卡片控件 - 符合 MVVM 的可复用控件
    /// </summary>
    public partial class ThumbnailCard : System.Windows.Controls.UserControl
    {
        // 依赖属性：笔记类型
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(
                nameof(Type),
                typeof(string),
                typeof(ThumbnailCard),
                new PropertyMetadata(NoteType.CommonName, OnTypeChanged));

        private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ThumbnailCard card)
            {
                card.CoerceValue(ContentPreviewProperty);
            }
        }

        // 依赖属性：创建日期
        public static readonly DependencyProperty CreateDateProperty =
            DependencyProperty.Register(
                nameof(CreateDate),
                typeof(DateTime),
                typeof(ThumbnailCard),
                new PropertyMetadata(DateTime.Now));

        // 依赖属性：标题
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(ThumbnailCard),
                new PropertyMetadata(""));

        // 依赖属性：内容预览（最多两行）
        public static readonly DependencyProperty ContentPreviewProperty =
            DependencyProperty.Register(
                nameof(ContentPreview),
                typeof(string),
                typeof(ThumbnailCard),
                new PropertyMetadata(string.Empty, null, CoerceContentPreview));

        private static object CoerceContentPreview(DependencyObject d, object baseValue)
        {
            if (d is ThumbnailCard card && string.Equals(card.Type, NoteType.RichTextName, StringComparison.OrdinalIgnoreCase))
            {
                return "暂不支持预览...";
            }
            return baseValue ?? string.Empty;
        }

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

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string ContentPreview
        {
            get => (string)GetValue(ContentPreviewProperty);
            set => SetValue(ContentPreviewProperty, value);
        }

        public ThumbnailCard()
        {
            InitializeComponent();
        }
    }
}