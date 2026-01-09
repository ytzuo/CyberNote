using CyberNote.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CyberNote.Services
{
    public static class RecordReader
    {
        public static async Task<List<Record>> LoadAllRecordsAsync(string filePath)
        {
            if (!File.Exists(filePath)) return new List<Record>();
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                using var doc = await JsonDocument.ParseAsync(fs);
                var list = new List<Record>();
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    var record = JsonSerializer.Deserialize<Record>(el.GetRawText());
                    if (record != null)
                    {
                        list.Add(record);
                    }
                }
                return list;
            }
            catch
            {
                return new List<Record>();
            }
        }
    }
}
