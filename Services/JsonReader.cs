using CyberNote.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace CyberNote.Services
{
    public static class JsonReader
    {
        public static List<NoteCard> LoadAllCard(string filePath)
        {
            if (!File.Exists(filePath)) return new List<NoteCard>();
            using var doc = JsonDocument.Parse(File.ReadAllText(filePath));
            var list = new List<NoteCard>();
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                if (el.ValueKind != JsonValueKind.Object) continue;
                if (!el.TryGetProperty("Type", out var typeProp)) continue;
                var type = typeProp.GetString();
                var raw = el.GetRawText();
                switch (type)
                {
                    case "Common":
                        var common = JsonSerializer.Deserialize<CommonNote>(raw);
                        if (common != null) list.Add(common);
                        break;
                    case "List":
                        var listNote = JsonSerializer.Deserialize<ListNote>(raw);
                        if (listNote != null)
                        {
                            // 反序列化后补齐任务 Owner 引用
                            foreach (var t in listNote.Tasks)
                                t.Owner = listNote;
                            list.Add(listNote);
                        }
                        break;
                    // 可拓展其他类型
                }
            }
            return list;
        }
    }
}