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
            DataContext = note ?? throw new ArgumentNullException(nameof(note));
        }

        private void RichTextCard_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // DataContext 变化时加载内容到查看器
            LoadContentToViewer();
        }

        private void LoadContentToViewer()
        {
            if (DataContext is CommonNote note && !string.IsNullOrEmpty(note.Content))
            {
                try
                {
                    var doc = new FlowDocument();
                    var paragraph = new Paragraph(new Run(note.Content));
                    doc.Blocks.Add(paragraph);
                    ContentViewer.Document = doc;
                }
                catch { }
            }
        }

        private void LoadContentToEditor()
        {
            if (DataContext is CommonNote note && !string.IsNullOrEmpty(note.Content))
            {
                try
                {
                    var doc = new FlowDocument();
                    var paragraph = new Paragraph(new Run(note.Content));
                    doc.Blocks.Add(paragraph);
                    ContentEditor.Document = doc;
                }
                catch { }
            }
        }

        private void SaveContentFromEditor()
        {
            if (DataContext is CommonNote note)
            {
                try
                {
                    var range = new TextRange(ContentEditor.Document.ContentStart, ContentEditor.Document.ContentEnd);
                    note.Content = range.Text;
                    LoadContentToViewer();
                }
                catch { }
            }
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
