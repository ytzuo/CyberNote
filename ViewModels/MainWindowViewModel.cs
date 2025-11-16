using CyberNote.Utils;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CyberNote.Models;

namespace CyberNote.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        /* 缩略图卡片列表 */
        public ObservableCollection<ThumbnailCardViewModel> ThumbnailCards { get; } = new ObservableCollection<ThumbnailCardViewModel>();

        /* 添加新卡片 */
        public ICommand AddNewCardCommand { get; }
        public event PropertyChangedEventHandler? PropertyChanged;

        /* 替换上方卡片 */
        public ICommand ReplaceMainCard { get; }


        private void ExecuteAddNewCard()
        {
            var newCard = new ThumbnailCardViewModel
            {
                Type = "Common",
                CreateDate = DateTime.Now,
                Title = "新笔记标题",
                Text1 = "点击编辑内容...",
            };
            // 构造对应的 Note 模型，便于后续展示主卡片
            var note = new CommonNote(newCard.Title, DateTime.Now, 0, newCard.Text1) { createDate = newCard.CreateDate };
            newCard.Note = note;

            ThumbnailCards.Add(newCard);
            Debug.WriteLine($"AddNewCard clicked: Title={newCard.Title}, Type={newCard.Type}");
        }

        private FrameworkElement? _mainCardElement;
        public FrameworkElement? MainCardElement
        {
            get => _mainCardElement;
            set { _mainCardElement = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainCardElement))); }
        }
        private void ReplaceMainCardExecute(ThumbnailCardViewModel vm)
        {
            Debug.WriteLine($"ReplaceMainCard executed: Title={vm?.Title}, Type={vm?.Type}");
            if (vm?.Note == null) return;
            MainCardElement = MainCardFactory.Create(vm.Note);
        }

        public MainWindowViewModel()
        {
            AddNewCardCommand = new RelayCommand(ExecuteAddNewCard);
            ReplaceMainCard = new RelayCommand<ThumbnailCardViewModel>(ReplaceMainCardExecute);

            /* 初始化测试卡片 */
            var c1 = new ThumbnailCardViewModel { 
                Type = "Common",
                CreateDate = DateTime.Now.AddDays(0),
                Title = "示例笔记111111",
                Text1 = "这是一个示例笔记的内容预览。",
                Text2 = "更多内容 more detail",
            };
            c1.Note = new CommonNote(c1.Title, DateTime.Now, 0, c1.Text1) { createDate = c1.CreateDate };

            var l1 = new ThumbnailCardViewModel
            {
                Type = "List",
                CreateDate = DateTime.Now.AddDays(-1),
                Title = "示例任务列表111111",
                Text1 = "这是一个示例任务列表的内容预览。",
                Text2 = "更多内容 more detail",
            };
            l1.Note = new ListNote(l1.Title, 0, l1.Text1, new List<TaskItem>()) { createDate = l1.CreateDate };

            var l2 = new ThumbnailCardViewModel
            {
                Type = "List",
                CreateDate = DateTime.Now.AddDays(-2),
                Title = "示例任务列表",
                Text1 = "这是一个示例任务列表的内容预览。",
                Text2 = "更多内容 more detail",
            };
            l2.Note = new ListNote(l2.Title, 0, l2.Text1, new List<TaskItem>()) { createDate = l2.CreateDate };

            ThumbnailCards.Add(c1);
            ThumbnailCards.Add(l1);
            ThumbnailCards.Add(l2);

            // 启动时选出日期最靠后的记录并显示为主卡片
            var latest = ThumbnailCards.OrderByDescending(x => x.CreateDate).FirstOrDefault();
            if (latest?.Note != null)
            {
                MainCardElement = MainCardFactory.Create(latest.Note);
            }
        }
    }
}
