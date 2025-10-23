using System.Windows;
using System.Windows.Input;

namespace CyberNote
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // 不再需要 SetToDesktop，使用 Topmost 实现组件效果
        }

        /// <summary>
        /// 当点击边框区域时，启动窗口拖动
        /// </summary>
        private void DragBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 确保是鼠标左键按下
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // 启动拖动操作
                this.DragMove();
            }
        }

        /// <summary>
        /// 测试按钮点击事件
        /// </summary>
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("按钮被点击了！\n窗口没有移动。", "测试", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}