using CyberNote.Models;
using CyberNote.Services;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;


namespace CyberNote.ViewModels
{
    class ThumbnailCardViewModel : INotifyPropertyChanged
    {
        // 完整数据模型
        public NoteCard? Note { get; set; }

        private string _type = NoteType.CommonName;
        private DateTime _createDate = DateTime.Now;
        private string _title = string.Empty;
        private string _contentPreview = string.Empty;
        private bool _isActive; // 是否当前主卡片

        public string Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        public DateTime CreateDate
        {
            get => _createDate;
            set
            {
                if (_createDate != value)
                {
                    _createDate = value;
                    OnPropertyChanged(nameof(CreateDate));
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        // 多行内容预览（最多两行，超过显示省略号）
        public string ContentPreview
        {
            get => _contentPreview;
            private set
            {
                if (_contentPreview != value)
                {
                    _contentPreview = value;
                    OnPropertyChanged(nameof(ContentPreview));
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        // 列表任务完成状态文本（仅当类型为 List 时显示）
        public string ListCompletionText
        {
            get
            {
                if (Note is ListNote ln)
                {
                    var total = ln.Tasks.Count;
                    if (total == 0) return "无任务";
                    var completed = ln.Tasks.Count(t => t.Progress);
                    return completed == total ? "已全部完成" : $"剩余 {total - completed} 项";
                }
                return string.Empty;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ThumbnailCardViewModel() { }

        public ThumbnailCardViewModel(NoteCard note)
        {
            Note = note;
            Title = note.Title;
            Type = note.Type;
            CreateDate = note.createDate != default ? note.createDate : DateTime.Now;
            
            WireNoteEvents();
            WireTaskEvents();
            BuildContentPreview();
            //DumpTestData();
        }

        /// <summary>
        /// 监听 Note 对象的属性变化，自动同步到 ViewModel
        /// </summary>
        private void WireNoteEvents()
        {
            if (Note is INotifyPropertyChanged notifyNote)
            {
                notifyNote.PropertyChanged += Note_PropertyChanged;
            }
        }

        /// <summary>
        /// 当 Note 的属性变化时，同步更新 ViewModel 的对应属性
        /// </summary>
        private void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(NoteCard.Title):
                    Title = Note?.Title ?? string.Empty;
                    break;
                case nameof(NoteCard.Content):
                    BuildContentPreview();
                    break;
                case nameof(CommonNote.createDate):
                    if (Note is CommonNote cn)
                        CreateDate = cn.createDate;
                    break;
            }
        }
       

        private void WireTaskEvents()
        {
            if (Note is ListNote ln)
            {
                foreach (var t in ln.Tasks)
                {
                    t.PropertyChanged += Task_PropertyChanged;
                }
                // 若后续支持动态增删，可再订阅 ln.Tasks.CollectionChanged
            }
        }

        private void Task_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TaskItem.Progress))
            {
                // 任务完成状态变化时刷新完成文本
                OnPropertyChanged(nameof(ListCompletionText));
            }
        }

        public void BuildContentPreview()
        {
            // 富文本直接使用占位提示
            if (string.Equals(Type, NoteType.RichTextName, StringComparison.OrdinalIgnoreCase))
            {
                ContentPreview = "暂不支持预览...";
                return;
            }

            var raw = Note?.Content ?? string.Empty;
            if (string.IsNullOrWhiteSpace(raw))
            {
                ContentPreview = string.Empty;
                return;
            }
            var lines = raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            ContentPreview = lines.Length <= 2 ? string.Join(Environment.NewLine, lines) : string.Join(Environment.NewLine, lines.Take(2)) + "...";
        }
    }
}
