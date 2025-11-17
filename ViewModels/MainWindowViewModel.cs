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
        public ObservableCollection<ThumbnailCardViewModel> ThumbnailCards { get; } = new ObservableCollection<ThumbnailCardViewModel>();

        public ICommand AddNewCardCommand { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand ReplaceMainCard { get; }

        private void ExecuteAddNewCard()
        {
            var content = "点击编辑内容...\n第二行示例";
            var note = new CommonNote("新笔记标题", DateTime.Now, 0, content) { createDate = DateTime.Now };
            var newCard = new ThumbnailCardViewModel(note)
            {
                Type = note.Type,
                CreateDate = note.createDate,
                Title = note.Title,
            };
            newCard.BuildContentPreview();
            ThumbnailCards.Add(newCard);
            SetActiveCard(newCard);
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
            if (vm?.Note == null) return;
            MainCardElement = MainCardFactory.Create(vm.Note);
            SetActiveCard(vm);
        }

        private void SetActiveCard(ThumbnailCardViewModel active)
        {
            foreach (var card in ThumbnailCards)
                card.IsActive = card == active;
        }

        public MainWindowViewModel()
        {
            AddNewCardCommand = new RelayCommand(ExecuteAddNewCard);
            ReplaceMainCard = new RelayCommand<ThumbnailCardViewModel>(ReplaceMainCardExecute);

            // 初始化测试数据：普通笔记
            var note1 = new CommonNote("示例笔记111111", DateTime.Now, 0, "这是一个示例笔记的内容预览。\n更多内容 more detail\n第三行隐藏") { createDate = DateTime.Now.AddDays(0) };
            var c1 = new ThumbnailCardViewModel(note1) { Type = note1.Type, CreateDate = note1.createDate, Title = note1.Title };
            c1.BuildContentPreview();

            // 初始化测试数据：任务列表（包含多任务，部分已完成）
            var tasksA = new List<TaskItem>
            {
                new TaskItem("编写接口文档"),
                new TaskItem("实现登录功能") { Progress = true },
                new TaskItem("编写单元测试"),
                new TaskItem("修复已知 Bug") { Progress = true },
                new TaskItem("整理发布说明"),
            };
            var listContent1 = "项目开发任务列表\n包含后台与前端部分";
            var noteList1 = new ListNote("示例任务列表111111", 0, listContent1, tasksA) { createDate = DateTime.Now.AddDays(-1) };
            var l1 = new ThumbnailCardViewModel(noteList1) { Type = noteList1.Type, CreateDate = noteList1.createDate, Title = noteList1.Title };
            l1.BuildContentPreview();

            // 另一组任务列表：全部未完成
            var tasksB = new List<TaskItem>
            {
                new TaskItem("阅读技术方案"),
                new TaskItem("拆分开发子任务"),
                new TaskItem("安排评审会议"),
            };
            var listContent2 = "需求评审准备事项";
            var noteList2 = new ListNote("示例任务列表", 0, listContent2, tasksB) { createDate = DateTime.Now.AddDays(-2) };
            var l2 = new ThumbnailCardViewModel(noteList2) { Type = noteList2.Type, CreateDate = noteList2.createDate, Title = noteList2.Title };
            l2.BuildContentPreview();

            // 第三个任务列表：大量任务（测试滚动与样式）
            var tasksC = new List<TaskItem>();
            for (int i = 1; i <= 15; i++)
            {
                tasksC.Add(new TaskItem($"批量任务 #{i}"));
            }
            tasksC[3].Progress = true;
            tasksC[7].Progress = true;
            tasksC[10].Progress = true;
            var listContent3 = "批量导入测试任务";
            var noteList3 = new ListNote("批量任务列表", 0, listContent3, tasksC) { createDate = DateTime.Now.AddDays(-3) };
            var l3 = new ThumbnailCardViewModel(noteList3) { Type = noteList3.Type, CreateDate = noteList3.createDate, Title = noteList3.Title };
            l3.BuildContentPreview();

            ThumbnailCards.Add(c1);
            ThumbnailCards.Add(l1);
            ThumbnailCards.Add(l2);
            ThumbnailCards.Add(l3);

            var latest = ThumbnailCards.OrderByDescending(x => x.CreateDate).FirstOrDefault();
            if (latest?.Note != null)
            {
                MainCardElement = MainCardFactory.Create(latest.Note);
                SetActiveCard(latest);
            }
        }
    }
}
