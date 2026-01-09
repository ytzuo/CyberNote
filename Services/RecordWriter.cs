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
    }
}
