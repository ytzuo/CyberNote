using CyberNote.Models;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace CyberNote.Services
{
    public static class RecordWriter
    {
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        public static async Task SaveRecordAsync(string filePath, Record record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            await _fileLock.WaitAsync();
            try
            {
                var arr = await JsonWriter.LoadExistingArrayAsync(filePath);

                // 按日期去重: 移除相同日期的旧记录
                var targetDate = record.Date.ToString("yyyy-MM-dd");
                for (int i = arr.Count - 1; i >= 0; i--)
                {
                    if (arr[i] is JsonObject o &&
                        o.TryGetPropertyValue("Date", out var dateNode) &&
                        dateNode?.GetValue<string>() == targetDate)
                    {
                        arr.RemoveAt(i);
                    }
                }

                arr.Add(record.toJson());

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                await JsonWriter.WriteArrayAtomicAsync(filePath, arr, options);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        /// <summary>
        /// 确保今天的记录存在，如果不存在则创建
        /// </summary>
        public static async Task EnsureTodayRecordAsync(string filePath)
        {
            await _fileLock.WaitAsync();
            try
            {
                var arr = await JsonWriter.LoadExistingArrayAsync(filePath);
                var todayStr = DateTime.Now.ToString("yyyy-MM-dd");
                bool exists = false;

                foreach (var node in arr)
                {
                    if (node is JsonObject o &&
                        o.TryGetPropertyValue("Date", out var dateNode) &&
                        dateNode?.GetValue<string>() == todayStr)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    var newRecord = new Record
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now),
                        Mood = MoodType.Unknown,
                        CardCount = 0,
                        Comment = "还没有记录今天的心情哦"
                    };
                    arr.Add(newRecord.toJson());

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    await JsonWriter.WriteArrayAtomicAsync(filePath, arr, options);
                }
            }
            finally
            {
                _fileLock.Release();
            }
        }

        /// <summary>
        /// 增加今天记录的卡片计数
        /// </summary>
        public static async Task IncrementTodayCardCountAsync(string filePath)
        {
            await _fileLock.WaitAsync();
            try
            {
                var arr = await JsonWriter.LoadExistingArrayAsync(filePath);
                var todayStr = DateTime.Now.ToString("yyyy-MM-dd");
                bool found = false;

                foreach (var node in arr)
                {
                    if (node is JsonObject o &&
                        o.TryGetPropertyValue("Date", out var dateNode) &&
                        dateNode?.GetValue<string>() == todayStr)
                    {
                        // 找到今天的记录，增加计数
                        if (o.TryGetPropertyValue("CardCount", out var countNode))
                        {
                            var count = countNode?.GetValue<int>() ?? 0;
                            o["CardCount"] = count + 1;
                        }
                        else
                        {
                            o["CardCount"] = 1;
                        }
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // 没找到，创建新记录，计数为 1
                    var newRecord = new Record
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now),
                        Mood = MoodType.Unknown,
                        CardCount = 1,
                        Comment = "还没有记录今天的心情哦"
                    };
                    arr.Add(newRecord.toJson());
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                await JsonWriter.WriteArrayAtomicAsync(filePath, arr, options);
            }
            finally
            {
                _fileLock.Release();
            }
        }
    }
}
