using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Linq;

namespace CyberNote.Models
{
    //任务列表类
    public class ListNote : NoteCard, INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; } = "List";
        private string _title = "无标题";
        private int _priority;
        private string _content = string.Empty;
        private DateTime _schedule;
        public DateTime createDate { get; set; }

        // 公开 setter 供反序列化填充
        public ObservableCollection<TaskItem> Tasks { get; set; } = new();

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

        public ListNote(string id, string title, int priority, string content, IEnumerable<TaskItem> tasks)
        {
            Id = id;
            Title = title;
            Priority = priority;
            Content = content;
            Tasks = new ObservableCollection<TaskItem>();
            foreach (var t in tasks)
            {
                t.Owner = this;
                Tasks.Add(t);
            }
        }
        // Backward-compatible overload auto-generating id
        public ListNote(string title, int priority, string content, IEnumerable<TaskItem> tasks)
            : this(Guid.NewGuid().ToString(), title, priority, content, tasks) { }

        public void ShiftPriority(int newPriority) => Priority = newPriority;
        public void UpdateContent(string newContent) => Content = newContent;
        public void AddTask(TaskItem task) { if (task == null) return; task.Owner = this; Tasks.Add(task); }
        public void RemoveTask(TaskItem task) { if (task == null) return; Tasks.Remove(task); }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public JsonObject toJson() => new JsonObject
        {
            ["id"] = Id,
            ["Type"] = Type,
            ["Title"] = Title,
            ["Content"] = Content,
            ["createDate"] = createDate,
            ["Priority"] = Priority,
            ["Tasks"] = new JsonArray(Tasks.Select(t => t.toJson()).ToArray())
        };
    }
}
