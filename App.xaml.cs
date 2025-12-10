using System.Configuration;
using System.Data;
using System.Windows;

namespace CyberNote
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        // App.xaml.cs
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var window = new MainWindow();
            window.Show();
        }
    }

}
