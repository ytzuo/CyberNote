using CyberNote.Models;
using CyberNote.Services;
using CyberNote.UI;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace CyberNote.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ThumbnailCardViewModel> ThumbnailCards { get; } = new ObservableCollection<ThumbnailCardViewModel>();

        public ICommand AddNewCardCommand { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand ReplaceMainCard { get; }

        // 暴露数据文件路径，供保存使用
        public string DataFilePath { get; } = "C:\\Users\\zz\\Desktop\\Code\\C#\\CyberNote\\Data\\test_json.json";

        private void ExecuteAddNewCard()
        {
            var content = "点击编辑内容...\n第二行示例";
            var note = new CommonNote("新笔记标题", DateTime.Now, 0, content) 
                        { createDate = DateTime.Now };
            var newCard = new ThumbnailCardViewModel(note)
            {
                Type = note.Type,
                CreateDate = note.createDate,
                Title = note.Title,
            };
            newCard.BuildContentPreview();
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
            // 只关闭当前激活卡片中的任务编辑模式
            var previousActive = ThumbnailCards.FirstOrDefault(c => c.IsActive);
            if (previousActive?.Note is ListNote oldList)
            {
                foreach (var task in oldList.Tasks)
                {
                    if (task.IsEditing)
                        task.IsEditing = false;
                }
            }

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

            LoadCard();
        }

        private void LoadCard()
        {
            var path = DataFilePath;
            if (!File.Exists(path)) { Debug.WriteLine("[Debug] JSON 文件不存在: " + path); return; }
            
            //加载所有卡片存储的list
            var cards = JsonReader.LoadAllCard(path);
            cards = SortNoteCards(cards);

            var idSet = new HashSet<string>();
            foreach (var card in cards)
            {
                bool dup = !string.IsNullOrWhiteSpace(card.Id) && !idSet.Add(card.Id);
                if (dup) Debug.WriteLine($"[警告] 发现重复 Id: {card.Id}");
                var vm = new ThumbnailCardViewModel(card)
                {
                    Type = card.Type,
                    CreateDate = card.createDate,
                    Title = card.Title,
                };
                ThumbnailCards.Add(vm);
            }

            ReplaceMainCardExecute(ThumbnailCards.First());
            //DumpJsonDebug(cards);
        }

        // 排序选项（可扩展）
        public enum SortOption
        {
            ByDateDesc,
            ByDateAsc,
            //ByPriorityDesc,
            //ByPriorityAsc
        }

        // 当前排序策略（默认按时间降序）
        public SortOption CurrentSort { get; set; } = SortOption.ByDateDesc;

        /// <summary>
        /// 对 NoteCard 列表按当前排序规则排序（用于读取后决定初始顺序）
        /// </summary>
        private List<NoteCard> SortNoteCards(List<NoteCard> cards)
        {
            return CurrentSort switch
            {
                SortOption.ByDateAsc => cards.OrderBy(c => c.createDate).ToList(),
                SortOption.ByDateDesc => cards.OrderByDescending(c => c.createDate).ToList(),
                //SortOption.ByPriorityAsc => cards.OrderBy(c => c.Priority).ToList(),
                //SortOption.ByPriorityDesc => cards.OrderByDescending(c => c.Priority).ToList(),
                _ => cards
            };
        }



        private void DumpJsonDebug(List<NoteCard> cards)
        {
            Debug.WriteLine("================ DESERIALIZE ================");
            Debug.WriteLine($"总计反序列化笔记数: {cards.Count}");

            foreach (var card in cards)
            {
                if (card is CommonNote cn)
                {
                    Debug.WriteLine($"[Common] Id={cn.Id} Title={cn.Title} createDate={cn.createDate:yyyy-MM-dd HH:mm:ss} Priority={cn.Priority} Progress={(cn.Progress ? "完成" : "未完成")} Content='{cn.Content}'");
                }
                else if (card is ListNote ln)
                {
                    Debug.WriteLine($"[List] Id={ln.Id} Title={ln.Title} createDate={ln.createDate:yyyy-MM-dd HH:mm:ss} Priority={ln.Priority} Tasks={ln.Tasks.Count} Content='{ln.Content}'");
                    int i = 1;
                    foreach (var t in ln.Tasks)
                        Debug.WriteLine($"   - Task#{i++}: {(t.Progress ? "完成" : "未完成")} | {t.Content}");
                }
                else
                {
                    Debug.WriteLine($"[未知类型] Type={card.Type} Title={card.Title}");
                }
            }
            Debug.WriteLine("================ END DEBUG ================");
        }
    }
}
