using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CyberNote.Models
{
    //任务列表类
    public class ListNote : NoteCard, INotifyPropertyChanged
    {
        public string Type { get; } = "List";
        private string _title = "无标题";
        private int _priority;
        private string _content = string.Empty;
        private DateTime _schedule;
        public DateTime createDate { get; set; }

        // 使用 ObservableCollection 支持 UI 动态增删
        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public string Title
        {
            get => _title;
            set { if (_title != value) { _title = value; OnPropertyChanged(); } }
        }
        public int Priority
        {
            get => _priority;
            set { if (_priority != value) { _priority = value; OnPropertyChanged(); } }
        }
        public string Content
        {
            get => _content;
            set { if (_content != value) { _content = value; OnPropertyChanged(); } }
        }
        public DateTime Schedule
        {
            get => _schedule;
            set { if (_schedule != value) { _schedule = value; OnPropertyChanged(); } }
        }

        public ListNote() { }

        public ListNote(string T, int priority, string content, IEnumerable<TaskItem> tasks)
        {
            Title = T;
            Priority = priority;
            Content = content;

            foreach (var t in tasks)
            {
                t.Owner = this;
                Tasks.Add(t);
            }
            //Tasks.CollectionChanged += (s, e) => RefreshSchedule();
            //RefreshSchedule();
        }

        //internal void RefreshSchedule()
        //{
        //    var next = Tasks.FirstOrDefault(t => t.Progress == false);
        //    if (next != null)
        //        Schedule = next.Schedule;
        //}

        public void ShiftPriority(int newPriority) => Priority = newPriority;
        public void UpdateContent(string newContent) => Content = newContent;

        public void AddTask(TaskItem task)
        {
            if (task == null) return;
            task.Owner = this;
            Tasks.Add(task);
            //RefreshSchedule();
        }
        public void RemoveTask(TaskItem task)
        {
            if (task == null) return;
            Tasks.Remove(task);
            //RefreshSchedule();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
