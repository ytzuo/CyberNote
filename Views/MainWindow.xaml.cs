using CyberNote.Services;
using CyberNote.ViewModels;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using System;

namespace CyberNote
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;
        private bool _isExit;
        private ToolStripMenuItem _showHideMenuItem;
        private Views.FloatingBlock _floatingBlock;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _floatingBlock = new Views.FloatingBlock();
            try
            {
                // 优先尝试部署目录下的 favicon.ico
                var icoPath = Path.Combine(AppContext.BaseDirectory, "favicon.ico");
                if (File.Exists(icoPath))
                {
                    _notifyIcon.Icon = new System.Drawing.Icon(icoPath);
                }
                else
                {
                    // 如果没有独立的 ico，尝试从可执行文件中提取嵌入图标
                    var exePath = Path.Combine(AppContext.BaseDirectory, "CyberNote.exe");
                    if (File.Exists(exePath))
                    {
                        try
                        {
                            var extracted = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                            if (extracted != null)
                            {
                                _notifyIcon.Icon = extracted;
                            }
                            else
                            {
                                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                            }
                        }
                        catch
                        {
                            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                        }
                    }
                    else
                    {
                        _notifyIcon.Icon = System.Drawing.SystemIcons.Application; // 兜底
                    }
                }
            }
            catch
            {
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application; // 如果失败，使用默认图标
            }
            _notifyIcon.Text = "CyberNote";
            _notifyIcon.Visible = true;

            // 创建右键菜单
            var contextMenu = new ContextMenuStrip();
            _showHideMenuItem = new ToolStripMenuItem("隐藏", null, ShowHideWindow);
            contextMenu.Items.Add(_showHideMenuItem);
            contextMenu.Items.Add("退出", null, (s, e) => { ExitApplication(); });
            _notifyIcon.ContextMenuStrip = contextMenu;

            // 双击托盘图标显示窗口
            _notifyIcon.DoubleClick += (s, e) =>
            {
                if (!_isExit)
                {
                    Show();
                    WindowState = WindowState.Normal;
                    Activate();
                    _floatingBlock.Hide();
                    _showHideMenuItem.Text = "隐藏";
                }
            };

            SetAutoStart();
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
            if (ActivityPopup.IsOpen)
            {
                UpdateActivityPopupPlacement();

                var target = ActivityPopup.PlacementTarget;
                ActivityPopup.PlacementTarget = null;
                ActivityPopup.PlacementTarget = target;
                ActivityPopup.IsOpen = false;
                ActivityPopup.IsOpen = true;
            }
            _lastLocation = new System.Windows.Point(Left, Top);
        }

        private void UpdateActivityPopupPlacement()
        {
            if (ActivityPopup.Child is not FrameworkElement child) return;

            // 强制重新测量以获取准确宽度
            child.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            var popupWidth = child.DesiredSize.Width;

            // 获取当前屏幕的工作区
            var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            var screen = System.Windows.Forms.Screen.FromHandle(handle);

            // 获取 DPI缩放比例
            var source = PresentationSource.FromVisual(this);
            double dpiScale = 1.0;
            if (source != null && source.CompositionTarget != null)
            {
                dpiScale = source.CompositionTarget.TransformToDevice.M11;
            }

            // 将屏幕右边界转换为 WPF 坐标
            double screenRight = screen.WorkingArea.Right / dpiScale;

            double gap = 10;
            double windowRightEdge = this.Left + this.Width;

            // 计算如果在右侧显示所需的右边界位置
            double requiredRight = windowRightEdge + gap + popupWidth;

            if (requiredRight > screenRight)
            {
                // 空间不足，移动到左侧
                if (ActivityPopup.Placement != PlacementMode.Left)
                {
                    ActivityPopup.Placement = PlacementMode.Left;
                    ActivityPopup.HorizontalOffset = -10;
                }
            }
            else
            {
                // 空间充足，保持在右侧
                if (ActivityPopup.Placement != PlacementMode.Right)
                {
                    ActivityPopup.Placement = PlacementMode.Right;
                    ActivityPopup.HorizontalOffset = 10;
                }
            }
        }

        private void ExtendBtn_Clicked(object sender, RoutedEventArgs e)
        {
            // 切换 Popup 显示
            WidePopup.IsOpen = !WidePopup.IsOpen;
        }

        // 窗口加载时记录初始位置
        private System.Windows.Point _lastLocation;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _lastLocation = new System.Windows.Point(Left, Top);
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

        private void FloatingActivityButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ActivityPopup.IsOpen)
            {
                UpdateActivityPopupPlacement();
                ActivityPopup.IsOpen = true;
            }
            else
            {
                ActivityPopup.IsOpen = false;
            }
        }

        /// <summary>
        /// 当用户选择某个卡片类型时触发
        /// </summary>
        private async void CardTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element || element.Tag is not string cardType)
                return;

            if (DataContext is not MainWindowViewModel vm)
                return;

            // 根据选择的类型创建新卡片
            switch (cardType)
            {
                case NoteType.CommonName:
                    await CreateCommonNote(vm);
                    break;
                case NoteType.ListName:
                    await CreateListNote(vm);
                    break;
                case NoteType.RichTextName:
                    await CreateRichTextNote(vm);
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
            var dlg = new Microsoft.Win32.OpenFileDialog
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

        private async Task CreateCommonNote(MainWindowViewModel vm)
        {
            var content = "双击编辑内容...\n第二行示例";
            var note = new Models.CommonNote("新随手记", DateTime.Now, 0, content) 
                    { createDate = DateTime.Now };
            var newCard = new ThumbnailCardViewModel(note)
            {
                Type = note.Type,
                CreateDate = note.createDate,
                Title = note.Title,
            };
            newCard.BuildContentPreview();
            
            // 插入到正确位置（降序时在开头）
            vm.ThumbnailCards.Insert(0, newCard);
            await JsonWriter.AppendNoteAsync(vm.DataFilePath, note);
            
            // 自动切换到新创建的卡片
            if (vm.ReplaceMainCard.CanExecute(newCard))
                vm.ReplaceMainCard.Execute(newCard);

        }

        private async Task CreateListNote(MainWindowViewModel vm)
        {
            var note = new Models.ListNote("新任务列表", 0, "暂无任务", new List<Models.TaskItem>()) 
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
            
            // 插入到正确位置（降序时在开头）
            vm.ThumbnailCards.Insert(0, newCard);
            await JsonWriter.AppendNoteAsync(vm.DataFilePath, note);
            
            // 自动切换到新创建的卡片
            if (vm.ReplaceMainCard.CanExecute(newCard))
                vm.ReplaceMainCard.Execute(newCard);
        }

        private async Task CreateRichTextNote(MainWindowViewModel vm)
        {
            var content = "双击编辑内容...\n第二行示例";
            var note = new Models.RichTextNote("新富文本笔记", DateTime.Now, 0, content);
            var newCard = new ThumbnailCardViewModel(note)
            {
                Type = note.Type,
                CreateDate = note.createDate,
                Title = note.Title,
            };
            newCard.BuildContentPreview();

            // 插入到正确位置（降序时在开头）
            vm.ThumbnailCards.Insert(0, newCard);
            await JsonWriter.AppendNoteAsync(vm.DataFilePath, note);

            // 自动切换到新创建的卡片
            if (vm.ReplaceMainCard.CanExecute(newCard))
                vm.ReplaceMainCard.Execute(newCard);
        }


        /// <summary>
        /// 按种类筛选按钮点击：循环切换（全部 → 随手记 → 任务列表 → 全部...）
        /// </summary>
        private void FilterTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.FilterType = vm.FilterType switch
                {
                    "All" => NoteType.CommonName,
                    NoteType.CommonName => NoteType.ListName,
                    NoteType.ListName => NoteType.RichTextName,
                    NoteType.RichTextName => "All",
                    _ => "All"
                };
                FilterTypeText.Text = vm.FilterType switch
                {
                    "All" => "全部",
                    NoteType.CommonName => "随手记",
                    NoteType.ListName => "任务列表",
                    NoteType.RichTextName => "富文本",
                    _ => "全部"
                };
                typeFont.Text = vm.FilterType switch
                {
                    "All" => "≡",       // 所有类型图标
                    NoteType.CommonName => "📝",    // 随手记图标
                    NoteType.ListName => "✓",      // 任务列表图标
                    NoteType.RichTextName => "🅡",
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

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            // 如果不是主动退出（如通过菜单项点关闭），则隐藏窗口
            if (!_isExit)
            {
                e.Cancel = true; // 取消关闭事件
                Hide();
                _floatingBlock.Show();
                _showHideMenuItem.Text = "显示";
            }
            else
            {
                // 真正退出时，确保资源被释放
                try
                {
                    if (_floatingBlock != null && _floatingBlock.IsLoaded)
                    {
                        _floatingBlock.Close();
                    }
                }
                catch { }
                
                try
                {
                    if (_notifyIcon != null)
                    {
                        _notifyIcon.Visible = false;
                        _notifyIcon.Dispose();
                    }
                }
                catch { }
            }
        }

        private void SetAutoStart()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                var appPath = AppContext.BaseDirectory + "CyberNote.exe";
                key?.SetValue("CyberNote", $"\"{appPath}\"");
            }
            catch { }
        }

        private void ShowHideWindow(object sender, EventArgs e)
        {
            // 防止在退出过程中操作
            if (_isExit) return;

            if (IsVisible)
            {
                // 隐藏窗口时，关闭所有 popup
                TypeSelectorPopup.IsOpen = false;
                OptionPopup.IsOpen = false;
                WidePopup.IsOpen = false;
                ActivityPopup.IsOpen = false;
                
                Hide();
                _showHideMenuItem.Text = "显示";
                
                // 显示悬浮块
                _floatingBlock.Show();
            }
            else
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
                _showHideMenuItem.Text = "隐藏";
                // 不打开 popup
                
                // 隐藏悬浮块
                _floatingBlock.Hide();
            }
        }

        public void ExitApplication()
        {
            _isExit = true;
            
            // 关闭悬浮块
            try
            {
                if (_floatingBlock != null && _floatingBlock.IsLoaded)
                {
                    _floatingBlock.Close();
                }
            }
            catch { }
            
            // 释放托盘图标
            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                }
            }
            catch { }
            
            // 关闭应用程序
            System.Windows.Application.Current.Shutdown();
        }
    }
}