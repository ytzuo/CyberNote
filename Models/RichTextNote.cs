using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CyberNote.Services;

namespace CyberNote.Models
{
    public class RichTextNote : NoteCard
    {
        private string _id = string.Empty;
        private string _title = "无标题";
        private DateTime _schedule;
        private DateTime _createDate = DateTime.Now;
        private bool _progress = false;
        private int _priority;
        private string _rtfContent = string.Empty;
        public string Id
        {
            get => _id;
            set { if (_id != value) { _id = value; OnPropertyChanged(); } }
        }

        public string Type { get; } = NoteType.RichTextName;

        public string Title
        {
            get => _title;
            set { if (_title != value) { _title = value; OnPropertyChanged(); } }
        }

        public DateTime Schedule
        {
            get => _schedule;
            set { if (_schedule != value) { _schedule = value; OnPropertyChanged(); } }
        }

        public DateTime createDate
        {
            get => _createDate;
            set { if (_createDate != value) { _createDate = value; OnPropertyChanged(); } }
        }

        public bool Progress
        {
            get => _progress;
            set { if (_progress != value) { _progress = value; OnPropertyChanged(); } }
        }

        public int Priority
        {
            get => _priority;
            set { if (_priority != value) { _priority = value; OnPropertyChanged(); } }
        }

        public string Content
        {
            get => _rtfContent;
            set { if (_rtfContent != value) { _rtfContent = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //无参构造函数
        public RichTextNote() { }

        //有参构造函数
        public RichTextNote(string id, string title, DateTime schedule, int priority, string rtfContent)
        {
            _id = id;
            _title = title;
            _schedule = schedule;
            _priority = priority;
            _rtfContent = rtfContent;
            _progress = false; // 默认状态
        }

        // Backward-compatible overload auto-generating id
        public RichTextNote(string title, DateTime schedule, int priority, string content)
            : this(Guid.NewGuid().ToString(), title, schedule, priority, content) { }

        //切换完成情况
        public void shift_progress() => Progress = !Progress;
        //修改优先级
        public void shift_priority(int new_priority) => Priority = new_priority;
        //修改内容
        public void update_content(string new_content) => Content = new_content;
        //修改时间
        public void update_schedule(DateTime new_schedule) => Schedule = new_schedule;

        public JsonObject toJson() => new JsonObject
        {
            ["id"] = Id,
            ["Type"] = Type,
            ["Title"] = Title,
            ["Schedule"] = Schedule,
            ["createDate"] = createDate,
            ["Progress"] = Progress,
            ["Priority"] = Priority,
            ["Content"] = Content
        };
    }
}
