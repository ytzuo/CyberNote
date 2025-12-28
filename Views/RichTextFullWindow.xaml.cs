using CyberNote.Models;
using CyberNote.Services;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace CyberNote.Views
{
    /// <summary>
    /// RichTextFullWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RichTextFullWindow : Window, INotifyPropertyChanged
    {
        private bool _isEditMode = false;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (_isEditMode != value)
                {
                    _isEditMode = value;
                    OnPropertyChanged(nameof(IsEditMode));
                    OnPropertyChanged(nameof(IsViewMode));
                }
            }
        }
        public bool IsViewMode => !IsEditMode;

        private readonly RichTextNote _note;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public RichTextFullWindow(RichTextNote note)
        {
            _note = note ?? throw new ArgumentNullException(nameof(note));
            InitializeComponent();
            DataContext = _note; // 标题等直接绑定到 Note
            LoadViewer();
        }

        private void LoadViewer()
        {
            Viewer.Document = CreateFlowDocumentFromRtf(_note.Content);
        }

        private void LoadEditor()
        {
            Editor.Document = CreateFlowDocumentFromRtf(_note.Content);
        }

        private async void SaveFromEditor()
        {
            try
            {
                // 更新模型内容为 RTF
                _note.Content = SaveRtfFromRichTextBox(Editor);
                if (string.IsNullOrWhiteSpace(_note.Id)) _note.Id = Guid.NewGuid().ToString();
                if (_note.createDate == default) _note.createDate = DateTime.Now;

                // 持久化到当前数据文件
                var path = ConfigService.DataFilePath;
                await JsonWriter.SaveNote(path, _note);

                // 保存后刷新查看文档
                LoadViewer();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveFromEditor failed: {ex.Message}");
            }
        }

        private static FlowDocument CreateFlowDocumentFromRtf(string? rtf)
        {
            var doc = new FlowDocument { PagePadding = new Thickness(0) };
            if (string.IsNullOrEmpty(rtf)) return doc;
            try
            {
                var tr = new TextRange(doc.ContentStart, doc.ContentEnd);
                using var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(rtf));
                tr.Load(ms, System.Windows.DataFormats.Rtf);
            }
            catch
            {
                doc.Blocks.Clear();
                doc.Blocks.Add(new Paragraph(new Run(rtf)));
            }
            return doc;
        }

        private static string SaveRtfFromRichTextBox(System.Windows.Controls.RichTextBox rtb)
        {
            rtb.Document.PagePadding = new Thickness(0);
            var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            using var ms = new System.IO.MemoryStream();
            range.Save(ms, System.Windows.DataFormats.Rtf);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private void ViewMode_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditMode) SaveFromEditor();
            IsEditMode = false;
        }

        private void EditMode_Click(object sender, RoutedEventArgs e)
        {
            LoadEditor();
            IsEditMode = true;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditMode) SaveFromEditor();
            DialogResult = true;
            Close();
        }

        private void FloatingInfoButton_Click(object sender, RoutedEventArgs e)
        {
            InfoPopup.IsOpen = !InfoPopup.IsOpen;
            RefreshInfoPopupPlacement();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            RefreshInfoPopupPlacement();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshInfoPopupPlacement();
        }

        private void RefreshInfoPopupPlacement()
        {
            if (InfoPopup == null) return;
            if (InfoPopup.IsOpen)
            {
                // 通过临时关闭再打开让 Popup 重算位置
                InfoPopup.IsOpen = false;
                InfoPopup.IsOpen = true;
            }
        }
    }
}
