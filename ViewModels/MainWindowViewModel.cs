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
using System.Windows.Input;
using System.Windows.Shapes;

namespace CyberNote.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ThumbnailCardViewModel> ThumbnailCards { get; } = new ObservableCollection<ThumbnailCardViewModel>();
        public ObservableCollection<ThumbnailCardViewModel> FilteredThumbnailCards { get; } = new ObservableCollection<ThumbnailCardViewModel>();

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

        private void ExecuteDeleteCard(ThumbnailCardViewModel vm)
        {
            if (vm == null) return;
            NoteCard note = vm.Note!;
            if (note == null) return;
            String noteId = note.Id;
            bool wasActive = vm.IsActive;
            ThumbnailCards.Remove(vm);
            JsonWriter.DeleteNote(DataFilePath, noteId);
            // 如果删除的是当前激活的卡片，尝试激活列表中的第一张卡片
            if (wasActive && ThumbnailCards.Count > 0)
            {
                ExecuteReplaceMainCard(ThumbnailCards.First());
            }
            else if (ThumbnailCards.Count == 0)
            {
                MainCardElement = null; // 没有卡片时清空主卡片显示
            }
        }

        private void SetActiveCard(ThumbnailCardViewModel active)
        {
            foreach (var card in ThumbnailCards)
                card.IsActive = card == active;
        }

        public MainWindowViewModel()
        {
            AddNewCardCommand = new RelayCommand(ExecuteAddNewCard);
            ReplaceMainCard = new RelayCommand<ThumbnailCardViewModel>(ExecuteReplaceMainCard);
            DeleteCard = new RelayCommand<ThumbnailCardViewModel>(ExecuteDeleteCard);

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
            LoadCard();
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
            // 如果需要，更新当前激活卡片
            if (FilteredThumbnailCards.Any())
            {
                ExecuteReplaceMainCard(FilteredThumbnailCards.First());
            }
        }

        /// <summary>
        /// 应用筛选和排序
        /// </summary>
        private void ApplyFilters()
        {
            var filtered = ThumbnailCards.AsEnumerable();

            // 搜索筛选
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(vm => vm.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                                vm.ContentPreview.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // 类型筛选
            if (FilterType != "All")
            {
                filtered = filtered.Where(vm => vm.Type == FilterType);
            }

            // 排序
            filtered = CurrentSort switch
            {
                SortOption.ByDateAsc => filtered.OrderBy(vm => vm.CreateDate),
                SortOption.ByDateDesc => filtered.OrderByDescending(vm => vm.CreateDate),
                _ => filtered
            };

            FilteredThumbnailCards.Clear();
            foreach (var vm in filtered)
            {
                FilteredThumbnailCards.Add(vm);
            }
        }
    }
}
