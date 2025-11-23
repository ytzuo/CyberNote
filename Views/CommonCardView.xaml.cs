using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using CyberNote.Models;

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
        }

        /// <summary>
        /// 按 Enter 保存并退出编辑模式
        /// </summary>
        private void EditElement_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+Enter 保存并退出
                IsEditMode = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                // Esc 取消编辑（不保存，因为是双向绑定已经自动保存了）
                IsEditMode = false;
                e.Handled = true;
            }
        }
    }
}
