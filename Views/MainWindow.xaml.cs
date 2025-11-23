using CyberNote.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace CyberNote
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //private MainWindowViewModel vm;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
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
            // 切换类型选择 Popup 的显示状态
            TypeSelectorPopup.IsOpen = !TypeSelectorPopup.IsOpen;
        }

        /// <summary>
        /// 当用户选择某个卡片类型时触发
        /// </summary>
        private void CardTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element || element.Tag is not string cardType)
                return;

            if (DataContext is not MainWindowViewModel vm)
                return;

            // 根据选择的类型创建新卡片
            switch (cardType)
            {
                case "Common":
                    CreateCommonNote(vm);
                    break;
                case "List":
                    CreateListNote(vm);
                    break;
                // 可以继续添加其他类型
            }

            // 关闭类型选择 Popup
            TypeSelectorPopup.IsOpen = false;
        }

        private void CreateCommonNote(MainWindowViewModel vm)
        {
            var content = "点击编辑内容...\n第二行示例";
            var note = new Models.CommonNote("新随手记", DateTime.Now, 0, content) { createDate = DateTime.Now };
            var newCard = new ThumbnailCardViewModel(note)
            {
                Type = note.Type,
                CreateDate = note.createDate,
                Title = note.Title,
            };
            newCard.BuildContentPreview();
            vm.ThumbnailCards.Add(newCard);
            
            // 自动切换到新创建的卡片
            if (vm.ReplaceMainCard.CanExecute(newCard))
                vm.ReplaceMainCard.Execute(newCard);
        }

        private void CreateListNote(MainWindowViewModel vm)
        {
            var note = new Models.ListNote("新任务列表", 0, "双击添加任务", new List<Models.TaskItem>()) 
            { 
                createDate = DateTime.Now 
            };
            var newCard = new ThumbnailCardViewModel(note)
            {
                Type = note.Type,
                CreateDate = note.createDate,
                Title = note.Title,
            };
            newCard.BuildContentPreview();
            vm.ThumbnailCards.Add(newCard);
            
            // 自动切换到新创建的卡片
            if (vm.ReplaceMainCard.CanExecute(newCard))
                vm.ReplaceMainCard.Execute(newCard);
        }
    }
}