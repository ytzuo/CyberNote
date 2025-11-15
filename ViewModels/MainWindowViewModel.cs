using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
                Text0 = "新笔记标题",
                Text1 = "点击编辑内容...",
                // 其他字段可选
            };
            ThumbnailCards.Add(newCard);
        }

        private void ReplaceMainCardExecute(ThumbnailCardViewModel card)
        {
            // 将目前的主卡片重新添加到列表
            // 将选择的卡片设为主卡片
        }
        public MainWindowViewModel()
        {
            AddNewCardCommand = new RelayCommand(ExecuteAddNewCard);
            ReplaceMainCard = new RelayCommand<ThumbnailCardViewModel>(ReplaceMainCardExecute);

            /* 初始化测试卡片 */
            ThumbnailCards.Add(new ThumbnailCardViewModel { 
                Type = "Common",
                CreateDate = DateTime.Now.AddDays(0),
                Text0 = "示例笔记111111",
                Text1 = "这是一个示例笔记的内容预览。",
                Text2 = "更多内容 more detail",
            });
            ThumbnailCards.Add(new ThumbnailCardViewModel
            {
                Type = "ListNode",
                CreateDate = DateTime.Now.AddDays(-1),
                Text0 = "示例任务列表111111",
                Text1 = "这是一个示例任务列表的内容预览。",
                Text2 = "更多内容 more detail",
            });
            ThumbnailCards.Add(new ThumbnailCardViewModel
            {
                Type = "ListNode",
                CreateDate = DateTime.Now.AddDays(-2),
                Text0 = "示例任务列表",
                Text1 = "这是一个示例任务列表的内容预览。",
                Text2 = "更多内容 more detail",
            });
        }
    }
}
