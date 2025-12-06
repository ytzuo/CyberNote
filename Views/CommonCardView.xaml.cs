using CyberNote.Models;
using CyberNote.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Windows.Controls;
using System.Windows.Input;

namespace CyberNote.Views
{
    /// <summary>
    /// CommonCardView.xaml 的交互逻辑
    /// </summary>
    public partial class CommonCardView : UserControl, INotifyPropertyChanged
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

        public bool IsViewMode => !_isEditMode;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CommonCardView()
        {
            InitializeComponent();
        }

        public CommonCardView(CommonNote note)
        {
            InitializeComponent();
            DataContext = note ?? throw new ArgumentNullException(nameof(note));
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

        /// <summary>
        /// 编辑框失去焦点时退出编辑模式
        /// </summary>
        private void EditElement_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            IsEditMode = false;
            // 保存到 JSON：DataContext 绑定到 CommonNote，Content 已被更新
                if (DataContext is CommonNote note)
            {
                try
                {                 
                    var path = ConfigService.DataFilePath;
                    var dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    if (string.IsNullOrWhiteSpace(note.Id))
                        note.Id = Guid.NewGuid().ToString();

                    // 使用 SaveNote，它会清除旧条目并复用 AppendNote 追加新条目
                    JsonWriter.SaveNote(path, note);

                    Debug.WriteLine($"Note saved: Id={note.Id} Title={note.Title}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"保存笔记失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 按 Esc 保存并退出编辑模式（使用已有 AppendNote 实现）
        /// </summary>
        private void EditElement_KeyDown(object sender, KeyEventArgs e)
        {
           if (e.Key == Key.Escape)
            {
                // 退出编辑模式
                IsEditMode = false;
                e.Handled = true;

                // 保存到 JSON：DataContext 绑定到 CommonNote，Content 已被更新
                if (DataContext is CommonNote note)
                {
                    try
                    {
                        var path = ConfigService.DataFilePath;
                        var dir = Path.GetDirectoryName(path);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        if (string.IsNullOrWhiteSpace(note.Id))
                            note.Id = Guid.NewGuid().ToString();

                        // 使用 SaveNote，它会清除旧条目并复用 AppendNote 追加新条目
                        JsonWriter.SaveNote(path, note);

                        Debug.WriteLine($"Note saved: Id={note.Id} Title={note.Title}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"保存笔记失败: {ex.Message}");
                    }
                }
           }
        }
    }
}
