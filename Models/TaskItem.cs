// Models/TaskItem.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

namespace CyberNote.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        //private DateTime _schedule;
        private bool _progress;
        private string _content = string.Empty;
        private bool _isEditing; // 新增：是否处于编辑模式

        //public DateTime Schedule
        //{
        //    get => _schedule;
        //    set { _schedule = value; OnPropertyChanged(); }
        //}

        public bool Progress
        {
            get => _progress;
            set
            {
                var old = _progress;
                _progress = value;
                OnPropertyChanged();
                //// 如果是从未完成 → 完成，通知 Owner
                //if (!old && _progress)
                //    Owner?.RefreshSchedule();
            }
        }

        public string Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(); }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set { if (_isEditing != value) { _isEditing = value; OnPropertyChanged(); } }
        }

        internal ListNote? Owner { get; set; }

        public TaskItem() { }

        public TaskItem(string content)
        {
            //_schedule = schedule;
            _content = content;
            _progress = false;
        }

        /// <summary>
        /// 切换完成状态（UI 交互入口）
        /// </summary>
        public void ShiftProgress()
        {
            Progress = !Progress; // 会自动触发通知和 RefreshSchedule
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public JsonObject toJson()
        {
            var obj = new JsonObject
            {
                ["Content"] = Content,
                ["Progress"] = Progress
            };
            return obj;
        }
    }
}