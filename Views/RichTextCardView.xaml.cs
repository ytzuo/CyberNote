using System;
using System.Collections.Generic;
using System.ComponentModel;
using CyberNote.Models;
using CyberNote.Services;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Security.Permissions;
using System.Text;

namespace CyberNote.Views
{
    /// <summary>
    /// RichTextCard.xaml 的交互逻辑
    /// </summary>
    public partial class RichTextCardView : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private bool _isEditMode = false;

        public event PropertyChangedEventHandler? PropertyChanged;

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
                    
                    // 切换模式时同步内容
                    if (_isEditMode)
                    {
                        // 进入编辑模式：从 Viewer 加载到 Editor
                        LoadContentToEditor();
                    }
                    else
                    {
                        // 退出编辑模式：从 Editor 保存到 Viewer
                        SaveContentFromEditor();
                    }
                }
            }
        }
        
        public bool IsViewMode => !_isEditMode;

        protected void OnPropertyChanged(string? name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public RichTextCardView()
        {
            InitializeComponent();
            this.DataContextChanged += RichTextCard_DataContextChanged;
        }

        public RichTextCardView(RichTextNote note)
        {
            InitializeComponent();
            this.DataContextChanged += RichTextCard_DataContextChanged;
            DataContext = note ?? throw new ArgumentNullException(nameof(note));
            LoadContentToViewer();
        }

        private void RichTextCard_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LoadContentToViewer();
        }

        private void LoadContentToViewer()
        {
            if (DataContext is not RichTextNote note) { ContentViewer.Document = new FlowDocument(); return; }
            ContentViewer.Document = CreateFlowDocumentFromRtf(note.Content);
        }

        private void LoadContentToEditor()
        {
            if (DataContext is not RichTextNote note) { ContentEditor.Document = new FlowDocument(); return; }
            ContentEditor.Document = CreateFlowDocumentFromRtf(note.Content);
        }

        private void SaveContentFromEditor()
        {
            if (DataContext is not RichTextNote note) return;
            note.Content = SaveRtfFromRichTextBox(ContentEditor);
            LoadContentToViewer();
        }

        private static FlowDocument CreateFlowDocumentFromRtf(string? rtf)
        {
            var doc = new FlowDocument { PagePadding = new Thickness(0) };
            if (string.IsNullOrEmpty(rtf)) return doc;
            try
            {
                var tr = new TextRange(doc.ContentStart, doc.ContentEnd);
                using var ms = new MemoryStream(Encoding.UTF8.GetBytes(rtf));
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
            // 确保编辑器文档无额外页边距
            rtb.Document.PagePadding = new Thickness(0);
            var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            using var ms = new MemoryStream();
            range.Save(ms, System.Windows.DataFormats.Rtf);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        /// <summary>
        /// 双击标题或内容进入编辑模式
        /// </summary>
        private void ViewElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                IsEditMode = true;
                e.Handled = true;
            }
        }

        // 失去焦点时退出编辑模式并保存
        private void EditElement_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            IsEditMode = false;
            // 保存到 JSON
            if (DataContext is RichTextNote note)
            {
                _ = SaveNoteAsync(note);
            }
        }

        private async Task SaveNoteAsync(RichTextNote note)
        {
            try
            {
                var path = ConfigService.DataFilePath;
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (string.IsNullOrWhiteSpace(note.Id))
                    note.Id = Guid.NewGuid().ToString();

                await JsonWriter.SaveNote(path, note);

                Debug.WriteLine($"Note saved: Id={note.Id} Title={note.Title}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存笔记失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 按 Esc 保存并退出编辑模式
        /// </summary>
        private void EditElement_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IsEditMode = false;
                e.Handled = true;

                if (DataContext is RichTextNote note)
                {
                    _ = SaveNoteAsync(note);
                }
            }
        }

        private void EnlargeBtn_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"[EnlargeBtn_Click] 开始，DataContext={(DataContext is RichTextNote ? "RichTextNote" : "非 RichTextNote")}");

            if (DataContext is RichTextNote note)
            {
                var dlg = new RichTextFullWindow(note)
                {
                    Owner = Window.GetWindow(this)
                };
                dlg.ShowDialog();
                // 对话框关闭后刷新小卡片的查看内容
                LoadContentToViewer();
            }
        }
    }
}
