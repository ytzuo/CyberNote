using CyberNote.Services;
using CyberNote.ViewModels;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

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

        private void FloatingAddButton_Click(object sender, RoutedEventArgs e)
        {
            // 切换类型选择 Popup 的显示状态
            TypeSelectorPopup.IsOpen = !TypeSelectorPopup.IsOpen;
        }

        private void FloatingOptionButton_Click(object sender, RoutedEventArgs e)
        {
            // 切换类型选择 Popup 的显示状态
            OptionPopup.IsOpen = !OptionPopup.IsOpen;
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

        /// <summary>
        /// 当用户选择修改数据文件路径时触发
        /// </summary>
        private void ChangePathButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "选择便签数据 JSON 文件",
                Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            // 设置默认打开路径为当前数据文件的目录
            if (DataContext is MainWindowViewModel vm && !string.IsNullOrWhiteSpace(vm.DataFilePath))
            {
                var dir = Path.GetDirectoryName(vm.DataFilePath);
                if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                {
                    dlg.InitialDirectory = dir;
                }
            }
            else
            {
                // 可选：如果没有当前路径，设置为用户文档文件夹
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            if (dlg.ShowDialog(this) == true)
            {
                if (DataContext is MainWindowViewModel mvm)
                {
                    mvm.DataFilePath = dlg.FileName;
                    mvm.ReloadData();
                    OptionPopup.IsOpen = false;
                }
            }
        }

        private void CreateCommonNote(MainWindowViewModel vm)
        {
            var content = "点击编辑内容...\n第二行示例";
            var note = new Models.CommonNote("新随手记", DateTime.Now, 0, content) 
                    { createDate = DateTime.Now };
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

        // 当前筛选类型状态（0=全部, 1=随手记, 2=任务列表）
        private int _filterTypeState = 0;
        private readonly string[] _filterTypeLabels = { "全部", "随手记", "任务列表" };

        // 当前排序状态（true=降序, false=升序）
        private bool _sortDescending = true;

        /// <summary>
        /// 按种类筛选按钮点击：循环切换（全部 → 随手记 → 任务列表 → 全部...）
        /// </summary>
        private void FilterTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.FilterType = vm.FilterType switch
                {
                    "All" => "Common",
                    "Common" => "List",
                    "List" => "All",
                    _ => "All"
                };
                FilterTypeText.Text = vm.FilterType switch
                {
                    "All" => "全部",
                    "Common" => "随手记",
                    "List" => "任务列表",
                    _ => "全部"
                };
                typeFont.Text = vm.FilterType switch
                {
                    "All" => "≡",       // 所有类型图标
                    "Common" => "📝",    // 随手记图标
                    "List" => "✓",      // 任务列表图标
                    _ => "≡"
                };
            }
        }

        /// <summary>
        /// 按日期排序按钮点击：切换升序/降序
        /// </summary>
        private void SortDateButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ToggleSortDate();
                SortDateText.Text = vm.CurrentSort == CyberNote.ViewModels.MainWindowViewModel.SortOption.ByDateDesc ? "降序" : "升序";
            }
        }
    }
}