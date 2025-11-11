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
        private string _type = "Common";
        private DateTime _createDate = DateTime.Now;
        private string _text0 = "";
        private string _text1 = "";
        private string _text2 = "";
        private string _text3 = "";

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

        public string Text0
        {
            get => _text0;
            set
            {
                if (_text0 != value)
                {
                    _text0 = value;
                    OnPropertyChanged(nameof(Text0));
                }
            }
        }

        public string Text1
        {
            get => _text1;
            set
            {
                if (_text1 != value)
                {
                    _text1 = value;
                    OnPropertyChanged(nameof(Text1));
                }
            }
        }

        public string Text2
        {
            get => _text2;
            set
            {
                if (_text2 != value)
                {
                    _text2 = value;
                    OnPropertyChanged(nameof(Text2));
                }
            }
        }

        public string Text3
        {
            get => _text3;
            set
            {
                if (_text3 != value)
                {
                    _text3 = value;
                    OnPropertyChanged(nameof(Text3));
                }
            }
        }

        public ICommand OnClickCommand { get; }

        public ThumbnailCardViewModel()
        {
            OnClickCommand = new RelayCommand(ExecuteOnClick);
        }

        private void ExecuteOnClick()
        {
            // 在这里实现点击卡片时的逻辑
            // 切换主视图中显示的卡片, 将本卡片的信息传递给主视图模型
            // 再将本卡片从下方列表中移除, 等到被关闭时再添加回去
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // 通用触发方法
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
