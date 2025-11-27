using CyberNote.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CyberNote.Views
{
    public partial class TaskListView : UserControl
    {
        // 依赖属性：Tasks 集合，显式绑定任务列表
        public static readonly DependencyProperty TasksProperty = DependencyProperty.Register(
            nameof(Tasks),
            typeof(ObservableCollection<TaskItem>),
            typeof(TaskListView),
            new PropertyMetadata(null));

        // 外部数据源(ListNote中的Tasks)绑定到此属性
        public ObservableCollection<TaskItem>? Tasks
        {
            get => (ObservableCollection<TaskItem>?)GetValue(TasksProperty);
            set => SetValue(TasksProperty, value);
        }

        public TaskListView()
        {
            InitializeComponent();
            // 使用 Tag 储存一个 ICommand 简易实现：用 RoutedCommand 替代快速方案
            this.Loaded += TaskListView_Loaded;
        }

        private void TaskListView_Loaded(object sender, RoutedEventArgs e)
        {
            // 将编辑控制命令挂在 ItemsControl.Tag（简单方式，避免创建额外依赖属性）
            if (Content is ScrollViewer sv && sv.Content is ItemsControl ic)
            {
                ic.Tag = new EditCommand(this);
            }
        }

        private class EditCommand : ICommand
        {
            private readonly TaskListView _owner;
            public EditCommand(TaskListView owner) { _owner = owner; }
            public event EventHandler? CanExecuteChanged { add { } remove { } }
            public bool CanExecute(object? parameter) => parameter is TaskItem;
            public void Execute(object? parameter)
            {
                if (parameter is not TaskItem task) return;

                // 如果当前处于编辑模式：按 Esc 或 Ctrl+Enter 退出编辑
                if (task.IsEditing)
                {
                    task.IsEditing = false; // 已是双向绑定，内容自动更新
                }
                else
                {
                    // 进入编辑模式
                    task.IsEditing = true;
                }
            }
        }

        // 失去焦点时退出编辑模式
        private void TaskEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is TaskItem task)
            {
                task.IsEditing = false; // 双向绑定自动保存内容
            }
        }
    }
}
