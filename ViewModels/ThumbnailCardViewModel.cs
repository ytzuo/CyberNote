using CyberNote.Models;
using CyberNote.Utils;
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

namespace CyberNote.ViewModels
{
    class ThumbnailCardViewModel : INotifyPropertyChanged
    {
        // 完整数据模型
        public NoteCard? Note { get; set; }

        private string _type = "Common";
        private DateTime _createDate = DateTime.Now;
        private string _title = string.Empty;
        private string _contentPreview = string.Empty;

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
            CreateDate = (note as CommonNote)?.createDate != default ? (note as CommonNote)!.createDate : DateTime.Now;
            BuildContentPreview();
        }

        public void BuildContentPreview()
        {
            var raw = Note?.Content ?? string.Empty;
            if (string.IsNullOrWhiteSpace(raw))
            {
                ContentPreview = string.Empty;
                return;
            }
            var lines = raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 2)
            {
                ContentPreview = string.Join(Environment.NewLine, lines);
            }
            else
            {
                ContentPreview = string.Join(Environment.NewLine, lines.Take(2)) + "...";
            }
        }
    }
}
