using CyberNote.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CyberNote.Views
{
    /// <summary>
    /// ListCardView.xaml 的交互逻辑
    /// </summary>
    public partial class ListCardView : System.Windows.Controls.UserControl
    {

        public ListCardView()
        {
            InitializeComponent();
        }

        public ListCardView(ListNote note)
        {
            InitializeComponent();
            DataContext = note ?? throw new ArgumentNullException(nameof(note));
        }

        private void AddTaskBtn_Clicked(object sender, RoutedEventArgs e)
        {
            // 新增任务项到末尾
            if (DataContext is ListNote list)
            {
                var task = new TaskItem("新任务");
                list.AddTask(task);
                task.IsEditing = true; // 默认进入编辑模式，方便立即输入

                // 等待UI更新后滚动到底部
                Dispatcher.BeginInvoke(() =>
                {
                    var taskListView = FindChild<TaskListView>(this);
                    if (taskListView != null)
                    {
                        var scrollViewer = FindChild<ScrollViewer>(taskListView);
                        scrollViewer?.ScrollToEnd();
                    }
                }, DispatcherPriority.Background);
            }
        }

        // 递归查找指定类型的子元素
        private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed) return typed;
                var result = FindChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
