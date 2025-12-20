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
using System.Windows.Shapes;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Controls.Primitives;

namespace CyberNote.Views
{
    /// <summary>
    /// FloatingBlock.xaml 的交互逻辑
    /// </summary>
    public partial class FloatingBlock : Window
    {
        public FloatingBlock()
        {
            InitializeComponent();
            LoadIcon();
            // 设置位置到屏幕右下角
            this.Left = SystemParameters.WorkArea.Width - this.Width;
            this.Top = SystemParameters.WorkArea.Height - this.Height;
        }

        private void LoadIcon()
        {
            var faviconImage = (System.Windows.Controls.Image)this.FindName("FaviconImage");
            if (faviconImage == null) return;

            try
            {
                // 优先尝试部署目录下的 favicon.png
                var pngPath = System.IO.Path.Combine(AppContext.BaseDirectory, "favicon.png");
                if (File.Exists(pngPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(pngPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze(); // 冻结以提高性能和线程安全
                    faviconImage.Source = bitmap;
                    return;
                }
                
                // 如果没有 PNG，尝试 ICO
                var icoPath = System.IO.Path.Combine(AppContext.BaseDirectory, "favicon.ico");
                if (File.Exists(icoPath))
                {
                    // 使用 IconBitmapDecoder 来正确处理 ICO 文件的透明度
                    var decoder = new IconBitmapDecoder(new Uri(icoPath), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    if (decoder.Frames.Count > 0)
                    {
                        var frame = decoder.Frames[0];
                        if (frame.Format.BitsPerPixel == 32) // 确保支持透明度
                        {
                            faviconImage.Source = frame;
                            return;
                        }
                    }
                }
                
                // 如果没有独立的 ico，尝试从可执行文件中提取嵌入图标
                var exePath = System.IO.Path.Combine(AppContext.BaseDirectory, "CyberNote.exe");
                if (File.Exists(exePath))
                {
                    try
                    {
                        var extracted = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                        if (extracted != null)
                        {
                            // 使用更好的转换方法
                            using (var bitmap = extracted.ToBitmap())
                            {
                                var hBitmap = bitmap.GetHbitmap();
                                try
                                {
                                    var imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                        hBitmap,
                                        IntPtr.Zero,
                                        System.Windows.Int32Rect.Empty,
                                        BitmapSizeOptions.FromEmptyOptions());
                                    imageSource.Freeze();
                                    faviconImage.Source = imageSource;
                                }
                                finally
                                {
                                    DeleteObject(hBitmap);
                                }
                            }
                        }
                    }
                    catch
                    {
                        faviconImage.Source = null;
                    }
                }
            }
            catch
            {
                faviconImage.Source = null;
            }
        }
        
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 确保是鼠标左键按下
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // 启动拖动操作
                this.DragMove();
            }           
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var contextMenu = new ContextMenu();

            var showMenuItem = new MenuItem { Header = "显示" };
            showMenuItem.Click += (s, args) =>
            {
                // 显示主窗口
                if (System.Windows.Application.Current.MainWindow != null)
                {
                    System.Windows.Application.Current.MainWindow.Show();
                    System.Windows.Application.Current.MainWindow.WindowState = WindowState.Normal;
                    System.Windows.Application.Current.MainWindow.Activate();
                }
                // 隐藏悬浮块
                this.Hide();
            };
            contextMenu.Items.Add(showMenuItem);

            var exitMenuItem = new MenuItem { Header = "关闭" };
            exitMenuItem.Click += (s, args) =>
            {
                // 退出应用程序
                if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.ExitApplication();
                }
            };
            contextMenu.Items.Add(exitMenuItem);

            // 设置菜单位置
            contextMenu.PlacementTarget = this;
            contextMenu.Placement = PlacementMode.MousePoint;
            contextMenu.IsOpen = true;
            e.Handled = true;
        }
    }
}
