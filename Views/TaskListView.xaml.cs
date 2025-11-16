using CyberNote.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

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
        }
    }
}
