using CyberNote.Models;
using System;
using System.Windows;
using System.Windows.Documents;

namespace CyberNote.Views
{
    /// <summary>
    /// RichTextFullWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RichTextFullWindow : Window
    {
        public bool IsEditMode { get; set; } = false;
        public bool IsViewMode => !IsEditMode;

        private readonly RichTextNote _note;

        public RichTextFullWindow(RichTextNote note)
        {
            _note = note ?? throw new ArgumentNullException(nameof(note));
            InitializeComponent();
            DataContext = this;
            LoadViewer();
        }

        private void LoadViewer()
        {
            var doc = new FlowDocument();
            doc.Blocks.Add(new Paragraph(new Run(_note.Content ?? string.Empty)));
            Viewer.Document = doc;
        }

        private void LoadEditor()
        {
            var doc = new FlowDocument();
            doc.Blocks.Add(new Paragraph(new Run(_note.Content ?? string.Empty)));
            Editor.Document = doc;
        }

        private void SaveFromEditor()
        {
            var range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
            _note.Content = range.Text;
            LoadViewer();
        }

        private void ViewMode_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditMode) SaveFromEditor();
            IsEditMode = false;
            RefreshMode();
        }

        private void EditMode_Click(object sender, RoutedEventArgs e)
        {
            LoadEditor();
            IsEditMode = true;
            RefreshMode();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditMode) SaveFromEditor();
            DialogResult = true;
            Close();
        }

        private void RefreshMode()
        {
            // 强制更新绑定
            DataContext = null;
            DataContext = this;
        }
    }
}
