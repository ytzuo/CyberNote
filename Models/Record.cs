using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;

namespace CyberNote.Models
{
    public class Record : INotifyPropertyChanged
    {
        private DateOnly _date;
        private MoodType _mood = MoodType.Unknown;
        private int _cardCount = 0;
        private string _comment = "还没有记录今天的心情哦";

        public DateOnly Date
        {
            get => _date;
            set { if (_date != value) { _date = value; OnPropertyChanged(); } }
        }

        public MoodType Mood
        {
            get => _mood;
            set { if (_mood != value) { _mood = value; OnPropertyChanged(); } }
        }

        public int CardCount
        {
            get => _cardCount;
            set { if (_cardCount != value) { _cardCount = value; OnPropertyChanged(); } }
        }

        public string Comment
        {
            get => _comment;
            set { if (_comment != value) { _comment = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Record()
        {
            _date = DateOnly.FromDateTime(DateTime.Now);
        }

        public Record(DateOnly date, MoodType mood, int cardCount, string comment)
        {
            _date = date;
            _mood = mood;
            _cardCount = cardCount;
            _comment = comment;
        }

        public JsonObject toJson()
        {
            return new JsonObject
            {
                ["Date"] = Date.ToString("yyyy-MM-dd"),
                ["Mood"] = Mood.Name,
                ["CardCount"] = CardCount,
                ["Comment"] = Comment
            };
        }
    }
}
