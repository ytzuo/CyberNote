using CyberNote.Models;
using CyberNote.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CyberNote.Views
{
    public partial class TaskListView : System.Windows.Controls.UserControl
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
                task.IsEditing = !task.IsEditing;
                if (!task.IsEditing)
                {
                    _owner.SaveCurrentListNote();
                }
            }
        }

        // 失去焦点时退出编辑模式并保存
        private void TaskEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb && tb.DataContext is TaskItem task)
            {
                task.IsEditing = false;
                SaveCurrentListNote();
            }
        }

        // 勾选变化时保存，确保进度状态持久化
        private void TaskProgress_Changed(object sender, RoutedEventArgs e)
        {
            SaveCurrentListNote();
        }

        private void SaveCurrentListNote()
        {
            // DataContext 是 ListNote（在 ListCardView 的构造函数中设置）
            if (DataContext is ListNote list)
            {
                // 更新 Content 为第一个任务项的文本
                if (list.Tasks.Count > 0)
                {
                    list.Content = list.Tasks[0].Content;
                }
                else
                {
                    list.Content = "无任务";
                }
                
                // 从窗口的 DataContext 取保存路径
                if (Window.GetWindow(this)?.DataContext is CyberNote.ViewModels.MainWindowViewModel vm)
                {
                    var path = vm.DataFilePath;
                    JsonWriter.SaveNote(path, list);
                }
            }
        }
    }
}
