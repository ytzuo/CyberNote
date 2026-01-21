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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CyberNote.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ThumbnailCardViewModel> ThumbnailCards { get; } = new ObservableCollection<ThumbnailCardViewModel>();
        public ICollectionView ThumbnailCardsView { get; private set; }
        
        // HeatMap 数据集合
        public ObservableCollection<HeatMapItemViewModel> HeatMapItems { get; } = new ObservableCollection<HeatMapItemViewModel>();
        public ObservableCollection<string> HeatMapHeaders { get; } = new ObservableCollection<string>();

        public ICommand HeatMapItemClickCommand { get; }

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
            
            // 2. 增加计数
            await IncrementTodayCardCountAsync();

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

            // 初始化热力图点击命令
            HeatMapItemClickCommand = new RelayCommand<HeatMapItemViewModel>(OnHeatMapItemClicked);

            ThumbnailCardsView = CollectionViewSource.GetDefaultView(ThumbnailCards);
            ThumbnailCardsView.Filter = FilterPredicate;
            SetSortDescriptions(); // 自定义方法设置 SortDescriptions 根据 CurrentSort 进行排序

            // 统一进行异步初始化，并处理潜在异常
            InitializeApp();
        }

        private async void InitializeApp()
        {
            try
            {
                await LoadCardAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error] LoadCardAsync failed: {ex}");
            }

            // 下面两个方法内部已包含 try-catch，但为了保险起见，依然放在这里或者单独处理
            await LoadHeatMapDataAsync();
            await InitializeTodayRecordAsync();
        }

        public async Task InitializeTodayRecordAsync()
        {
            try
            {
                var rPath = ConfigService.RecordFilePath;
                await RecordWriter.EnsureTodayRecordAsync(rPath);
                // 刷新热力图以显示今天的数据（即使是空）
                await LoadHeatMapDataAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error] InitializeTodayRecordAsync: {ex.Message}");
            }
        }

        public async Task IncrementTodayCardCountAsync()
        {
            try
            {
                var rPath = ConfigService.RecordFilePath;
                await RecordWriter.IncrementTodayCardCountAsync(rPath);
                // 刷新热力图
                await LoadHeatMapDataAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error] IncrementTodayCardCountAsync: {ex.Message}");
            }
        }

        private async Task LoadHeatMapDataAsync()
        {
            try
            {
                var rPath = ConfigService.RecordFilePath;
                // 确保文件存在， RecordReader 内部已有判断，但为了安全
                var records = await RecordReader.LoadAllRecordsAsync(rPath);
                var recordDict = records.ToDictionary(r => r.Date);

                int weeksToShow = 10; // 显示过去10周
                var today = DateTime.Today;

                // 计算本周一的日期
                // DayOfWeek: Sunday=0, Monday=1, ..., Saturday=6
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                var currentWeekMonday = today.AddDays(-diff);

                // 起始日期：倒推 weeksToShow - 1 周的周一
                var startDate = currentWeekMonday.AddDays(-(weeksToShow - 1) * 7);

                var items = new List<HeatMapItemViewModel>();
                var headers = new List<string>();
                int lastLabeledMonth = -1;

                // 生成周表头
                for (int w = 0; w < weeksToShow; w++)
                {
                    var weekMonday = startDate.AddDays(w * 7);
                    var weekSunday = weekMonday.AddDays(6);
                    string label = "";
                    
                    // 如果这周的周一和周日都在同一个月（即第一个整列属于同一个月）
                    if (weekMonday.Month == weekSunday.Month)
                    {
                        // 如果这个月还没被标记过
                        if (weekMonday.Month != lastLabeledMonth)
                        {
                            label = DateName.FromMonth(weekMonday.Month).Name;
                            lastLabeledMonth = weekMonday.Month;
                        }
                    }
                    headers.Add(label);
                }

                for (int i = 0; i < weeksToShow * 7; i++)
                {
                    var date = startDate.AddDays(i);
                    var dateOnly = DateOnly.FromDateTime(date);

                    var item = new HeatMapItemViewModel
                    {
                        Date = date,
                        Color = "#EBEDF0", // 默认颜色
                        Count = 0,
                        Mood = MoodType.Unknown.Emoji,
                        NoteSummary = "无记录"
                    };

                    if (recordDict.TryGetValue(dateOnly, out var record))
                    {
                        item.Count = record.CardCount;
                        item.Mood = record.Mood?.Emoji ?? MoodType.Unknown.Emoji;
                        item.NoteSummary = record.Comment;
                        item.Color = GetColorForCount(record.CardCount);
                    }

                    items.Add(item);
                }

                // 需要在 UI 线程更新 ObservableCollection
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    HeatMapHeaders.Clear();
                    foreach (var h in headers)
                    {
                        HeatMapHeaders.Add(h);
                    }

                    HeatMapItems.Clear();
                    foreach (var item in items)
                    {
                        HeatMapItems.Add(item);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error] LoadHeatMapDataAsync: {ex.Message}");
            }
        }

        private string GetColorForCount(int count)
        {
            if (count == 0) return "#EBEDF0";
            if (count <= 2) return "#9BE9A8"; // 浅绿
            if (count <= 5) return "#40C463"; // 中绿
            if (count <= 9) return "#30A14E"; // 深绿
            return "#216E39"; // 最深绿
        }

        private async Task HandleHeatMapItemClickedAsync(HeatMapItemViewModel item)
        {
            if (item == null) return;
            try
            {
                await OpenRecordEditDialogAsync(item);
            }
            catch (Exception ex)
            {
                // 捕获并记录异常，防止未处理异常导致应用程序崩溃
                Debug.WriteLine($"Error while opening record edit dialog: {ex}");
                System.Windows.MessageBox.Show(
                    "打开记录编辑窗口时发生错误，请重试或查看日志获取更多信息。",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OnHeatMapItemClicked(HeatMapItemViewModel item)
        {
            if (item == null) return;

            // 使用异步任务来执行，避免UI线程阻塞
            // 注意：MvvmLight relaycommand 是 void 委托，这里用 fire-and-forget
            _ = HandleHeatMapItemClickedAsync(item);
        }

        private async Task OpenRecordEditDialogAsync(HeatMapItemViewModel item)
        {
            // 1. 从文件加载所有记录以找到准确的 Record 对象
            var rPath = ConfigService.RecordFilePath;
            List<Record> records = new List<Record>();
            try
            {
                records = await RecordReader.LoadAllRecordsAsync(rPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load records from '{rPath}': {ex}");
            }

            var dateOnly = DateOnly.FromDateTime(item.Date);
            var record = records.FirstOrDefault(r => r.Date == dateOnly);

            // 2. 如果不存在，创建一个新的（但不立即保存到文件，除非用户点击保存）
            if (record == null)
            {
                record = new Record
                {
                    Date = dateOnly,
                    Mood = MoodType.Unknown,
                    CardCount = 0,
                    Comment = ""
                };
            }

            // 3. 在 UI 线程打开窗口
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var dlg = new RecordEditWindow(record);
                // 设置 owner 为主窗口（如果能获取到），或者居中
                if (System.Windows.Application.Current.MainWindow != null)
                {
                    dlg.Owner = System.Windows.Application.Current.MainWindow;
                }

                if (dlg.ShowDialog() == true)
                {
                    // 用户点击了保存，更新 record 对象
                    record.Mood = dlg.SelectedMood;
                    record.Comment = dlg.Comment;

                    // 4. 写回文件
                    _ = SaveRecordAndReloadAsync(record);
                }
            });
        }

        private async Task SaveRecordAndReloadAsync(Record record)
        {
            try
            {
                var rPath = ConfigService.RecordFilePath;
                await RecordWriter.SaveRecordAsync(rPath, record);
                
                // 刷新热力图
                await LoadHeatMapDataAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error] SaveRecordAndReloadAsync: {ex.Message}");
            }
        }

        private async Task LoadCardAsync()
        {
            try
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
            catch (Exception ex)
            {
                 Debug.WriteLine($"[Error] LoadCardAsync: {ex}");
            }
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

    public class HeatMapItemViewModel : INotifyPropertyChanged
    {
        public DateTime Date { get; set; }
        
        private string _color = "#EBEDF0"; // 默认浅灰色
        public string Color 
        { 
            get => _color; 
            set 
            {
                _color = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color)));
            }
        }

        public int Count { get; set; }
        public string Mood { get; set; } = "";
        public string NoteSummary { get; set; } = "";

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
