using System;
using System.Windows.Controls;
using CyberNote.Models;

namespace CyberNote.Views
{
    /// <summary>
    /// CommonCardView.xaml 的交互逻辑
    /// </summary>
    public partial class CommonCardView : UserControl
    {
        public CommonCardView()
        {
            InitializeComponent();
        }
        public CommonCardView(CommonNote note)
        {
            InitializeComponent();
            DataContext = note ?? throw new ArgumentNullException(nameof(note));
        }
    }
}
