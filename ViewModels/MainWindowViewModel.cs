using CyberNote.Models;
using CyberNote.Services;
using CyberNote.UI;
using CyberNote.Views;
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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;

namespace CyberNote.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ThumbnailCardViewModel> ThumbnailCards { get; } = new ObservableCollection<ThumbnailCardViewModel>();
        public ICollectionView ThumbnailCardsView { get; private set; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchText)));
                    ApplyFilters();
                }
            }
        }

        private string _filterType = "All"; // "All", "Common", "List"
        public string FilterType
        {
            get => _filterType;
            set
            {
                if (_filterType != value)
                {
                    _filterType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterType)));
                    ApplyFilters();
                }
            }
        }

        public ICommand AddNewCardCommand { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand ReplaceMainCard { get; }

        public ICommand DeleteCard { get; }

        // 暴露数据文件路径，供保存使用（从配置服务读取/持久化）
        public string DataFilePath
        {
            get => ConfigService.DataFilePath;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                ConfigService.DataFilePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataFilePath)));
            }
        }

        private async Task ExecuteAddNewCard()
        {
            var content = "点击编辑内容...\n正文示例";
            var note = new CommonNote("新笔记标题", DateTime.Now, 0, content) 
                        { createDate = DateTime.Now };
            var newCard = new ThumbnailCardViewModel(note)
            {
                Type = note.Type,
                CreateDate = note.createDate,
                Title = note.Title,
            };
            newCard.BuildContentPreview();
            
            // 直接添加到开头（对于降序排序，最新的应该在最前面）
            ThumbnailCards.Insert(0, newCard);
            await JsonWriter.AppendNoteAsync(DataFilePath, note);
            
            Debug.WriteLine($"AddNewCard: Title={newCard.Title}, ThumbnailCards.Count={ThumbnailCards.Count}");
        }



        private FrameworkElement? _mainCardElement;
        public FrameworkElement? MainCardElement
        {
            get => _mainCardElement;
            set { _mainCardElement = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainCardElement))); }
        }
        private void ExecuteReplaceMainCard(ThumbnailCardViewModel vm)
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

        private async Task ExecuteDeleteCard(ThumbnailCardViewModel vm)
        {
            if (vm == null) return;
            NoteCard note = vm.Note!;
            if (note == null) return;
            String noteId = note.Id;
            bool wasActive = vm.IsActive;
            ThumbnailCards.Remove(vm);
            await JsonWriter.DeleteNote(DataFilePath, noteId);
            ApplyFilters(); // 更新筛选后的列表
            // 如果删除的是当前激活的卡片，尝试激活列表中的第一张卡片
            if (wasActive && ThumbnailCards.Count > 0)
            {
                ExecuteReplaceMainCard(ThumbnailCards.First());
            }
            else if (ThumbnailCards.Count == 0)
            {
                await ExecuteAddNewCard();
                ExecuteReplaceMainCard(ThumbnailCards.First());
            }
        }

        private void SetActiveCard(ThumbnailCardViewModel active)
        {
            foreach (var card in ThumbnailCards)
                card.IsActive = card == active;
        }

        public MainWindowViewModel()
        {
            AddNewCardCommand = new RelayCommand(async () => await ExecuteAddNewCard());
            ReplaceMainCard = new RelayCommand<ThumbnailCardViewModel>(ExecuteReplaceMainCard);
            DeleteCard = new RelayCommand<ThumbnailCardViewModel>(async (vm) => await ExecuteDeleteCard(vm));

            ThumbnailCardsView = CollectionViewSource.GetDefaultView(ThumbnailCards);
            ThumbnailCardsView.Filter = FilterPredicate;
            SetSortDescriptions(); // 自定义方法设置 SortDescriptions 根据 CurrentSort 进行排序

            // 构造函数不能是异步的，所以这里不能 await LoadCard()
            // 我们可以启动一个不等待的任务，或者将 LoadCard 改回同步，或者在 Loaded 事件中调用
            // 这里选择启动一个不等待的任务，并处理可能的异常
            _ = LoadCardAsync();
        }

        private async Task LoadCardAsync()
        {
            var path = DataFilePath;
            if (!File.Exists(path)) { Debug.WriteLine("[Debug] JSON 文件不存在: " + path); return; }
            
            //加载所有卡片存储的list
            // JsonReader.LoadAllCard 目前是同步的，如果需要也可以改为异步
            var cards = await Task.Run(() => JsonReader.LoadAllCard(path));
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

            if (ThumbnailCards.Count == 0)
            {
                await ExecuteAddNewCard();
            }
            ExecuteReplaceMainCard(ThumbnailCards.First());
            // 初始化筛选后的列表
            ApplyFilters();
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
        /// 对 ThumbnailCardViewModel 列表按当前排序规则排序
        /// </summary>
        private List<ThumbnailCardViewModel> SortNoteCards(ObservableCollection<ThumbnailCardViewModel> cards)
        {
            return CurrentSort switch
            {
                SortOption.ByDateAsc => cards.OrderBy(c => c.CreateDate).ToList(),
                SortOption.ByDateDesc => cards.OrderByDescending(c => c.CreateDate).ToList(),
                //SortOption.ByPriorityAsc => cards.OrderBy(c => c.Priority).ToList(),
                //SortOption.ByPriorityDesc => cards.OrderByDescending(c => c.Priority).ToList(),
                _ => cards.ToList()
            };
        }

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

        /// <summary>
        /// 重新从当前数据文件读取并刷新 UI
        /// </summary>
        public void ReloadData()
        {
            ThumbnailCards.Clear();
            _ = LoadCardAsync();
        }

        /// <summary>
        /// 按日期排序的切换（升序/降序）
        /// </summary>
        public void ToggleSortDate()
        {
            CurrentSort = CurrentSort == SortOption.ByDateDesc ? SortOption.ByDateAsc : SortOption.ByDateDesc;
            // 重新排序现有卡片
            var sorted = SortNoteCards(ThumbnailCards);
            ThumbnailCards.Clear();
            foreach (var vm in sorted)
            {
                ThumbnailCards.Add(vm);
            }
            // 应用筛选和排序
            ApplyFilters();
        }

        private bool FilterPredicate(object obj)
        {
            if (obj is not ThumbnailCardViewModel vm) return false;

            // 类型筛选
            if (FilterType != "All" && vm.Type != FilterType) return false;

            // 搜索筛选
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText;
                return vm.Title.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                       vm.ContentPreview.Contains(s, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetSortDescriptions()
        {
            ThumbnailCardsView.SortDescriptions.Clear();
            if (CurrentSort == SortOption.ByDateDesc)
                ThumbnailCardsView.SortDescriptions.Add(new SortDescription(nameof(ThumbnailCardViewModel.CreateDate), ListSortDirection.Descending));
            else
                ThumbnailCardsView.SortDescriptions.Add(new SortDescription(nameof(ThumbnailCardViewModel.CreateDate), ListSortDirection.Ascending));
        }

        /// <summary>
        /// 应用筛选和排序
        /// </summary>
        private void ApplyFilters()
        {
            // 如果排序策略可能改变，先更新 SortDescriptions
            SetSortDescriptions();

            // 然后刷新 Filter
            ThumbnailCardsView.Refresh();
        }
    }
}
