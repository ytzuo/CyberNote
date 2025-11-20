using CyberNote.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CyberNote.Services
{
    public static class JsonReader
    {
        /// <summary>
        /// 完整读取文件，缺字段自动补默认值，绝不丢数据。
        /// </summary>
        public static List<NoteCard> LoadAllCard(string filePath)
        {
            if (!File.Exists(filePath)) return new List<NoteCard>();

            var list = new List<NoteCard>();
            using var doc = JsonDocument.Parse(File.ReadAllText(filePath));

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                if (el.ValueKind != JsonValueKind.Object) continue;
                if (!el.TryGetProperty("Type", out var typeProp)) continue;

                var type = typeProp.GetString();
                switch (type)
                {
                    case "Common":
                        list.Add(ReadCommonNote(el));
                        break;
                    case "List":
                        list.Add(ReadListNote(el));
                        break;
                        // 后续扩展新类型在这里加 case
                }
            }
            return list;
        }

        private static CommonNote ReadCommonNote(JsonElement el)
        {
            var note = new CommonNote();
            note.Title = el.GetPropertyString("Title") ?? "无标题";
            note.Content = el.GetPropertyString("Content") ?? string.Empty;
            note.Schedule = el.GetPropertyDateTime("Schedule");
            note.createDate = el.GetPropertyDateTime("createDate");
            note.Progress = el.GetPropertyBool("Progress");
            note.Priority = el.GetPropertyInt("Priority");
            return note;
        }

        private static ListNote ReadListNote(JsonElement el)
        {
            var note = new ListNote();
            note.Title = el.GetPropertyString("Title") ?? "无标题";
            note.Content = el.GetPropertyString("Content") ?? string.Empty;
            note.createDate = el.GetPropertyDateTime("createDate");
            note.Priority = el.GetPropertyInt("Priority");

            if (el.TryGetProperty("Tasks", out var tasksEl) && tasksEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var t in tasksEl.EnumerateArray())
                {
                    var task = new TaskItem
                    {
                        Content = t.GetPropertyString("Content") ?? string.Empty,
                        Progress = t.GetPropertyBool("Progress")
                    };
                    task.Owner = note;   // 关键：补引用
                    note.Tasks.Add(task);
                }
            }
            return note;
        }

        #region 小工具：安全读取各类值
        private static string? GetPropertyString(this JsonElement el, string name)
            => el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String
               ? v.GetString()
               : null;

        private static bool GetPropertyBool(this JsonElement el, string name, bool def = false)
            => el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.True;

        private static int GetPropertyInt(this JsonElement el, string name, int def = 0)
            => el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number
               ? v.GetInt32()
               : def;

        private static DateTime GetPropertyDateTime(this JsonElement el, string name)
            => el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String
               && DateTime.TryParse(v.GetString(), out var dt)
               ? dt
               : default;
        #endregion
    }
}

