using System;
using System.Windows;
using System.Windows.Controls;
using CyberNote.Models;
using CyberNote.Services;

namespace CyberNote.Views
{
    public partial class RecordEditWindow : Window
    {
        private readonly Record _record;

        public MoodType? SelectedMood => MoodCombo.SelectedItem as MoodType;
        public string Comment => CommentBox.Text;

        public RecordEditWindow(Record record)
        {
            InitializeComponent();
            _record = record ?? throw new ArgumentNullException(nameof(record));
            
            DateText.Text = $"{record.Date:yyyy年MM月dd日}  {GetWeekName(record.Date)}";
            
            // 初始化下拉框
            MoodCombo.ItemsSource = MoodType.All;
            MoodCombo.SelectedItem = record.Mood ?? MoodType.Unknown;
            
            CommentBox.Text = record.Comment;
            
            // Placeholder logic
            UpdatePlaceholder();
            CommentBox.TextChanged += (s, e) => UpdatePlaceholder();

            // 允许拖动
            MouseLeftButtonDown += (s, e) => DragMove();
        }

        private void UpdatePlaceholder()
        {
            PlaceholderText.Visibility = string.IsNullOrEmpty(CommentBox.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        private string GetWeekName(DateOnly date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => "周一",
                DayOfWeek.Tuesday => "周二",
                DayOfWeek.Wednesday => "周三",
                DayOfWeek.Thursday => "周四",
                DayOfWeek.Friday => "周五",
                DayOfWeek.Saturday => "周六",
                DayOfWeek.Sunday => "周日",
                _ => ""
            };
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
