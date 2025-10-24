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
        // 窗口移动时，手动更新 Popup 位置
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (WidePopup.IsOpen)
            {
                // 强制 Popup 重新计算位置
                WidePopup.PlacementTarget = null;
                WidePopup.PlacementTarget = ExpendBtn;
                WidePopup.IsOpen = false;
                WidePopup.IsOpen = true;
            }
            _lastLocation = new Point(Left, Top);
        }
        private void ExtendBtn_Clicked(object sender, RoutedEventArgs e)
        {
            // 切换 Popup 显示
            WidePopup.IsOpen = !WidePopup.IsOpen;
        }

        // 窗口加载时记录初始位置
        private Point _lastLocation;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _lastLocation = new Point(Left, Top);
        }

        private void FloatingButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}