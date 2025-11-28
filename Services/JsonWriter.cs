using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using CyberNote.Models;
using System.Linq;

namespace CyberNote.Services
{
    public static class JsonWriter
    {
        public static void AppendNote(string filePath, NoteCard note)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));

            JsonArray rootArray = LoadExistingArray(filePath);

            string newId = note.Id;
            bool exists = rootArray.Any(n => n is JsonObject o &&
                                             ((o.TryGetPropertyValue("Id", out var idNode) && idNode?.GetValue<string>() == newId) ||
                                              (o.TryGetPropertyValue("id", out var idNode2) && idNode2?.GetValue<string>() == newId)));
            if (exists) return; // 已存在同 ID

            rootArray.Add(note.toJson());
            WriteArray(filePath, rootArray);
        }

        //实现保存修改（替换或追加）
        public static void SaveNote(string filePath, NoteCard note)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));

            // 读取并移除所有同 Id 的现有项，再写回空缺的数组
            var arr = LoadExistingArray(filePath);

            if (!string.IsNullOrWhiteSpace(note.Id))
            {
                for (int i = arr.Count - 1; i >= 0; i--)
                {
                    if (arr[i] is JsonObject o &&
                        ((o.TryGetPropertyValue("Id", out var idNode) && idNode?.GetValue<string>() == note.Id) ||
                         (o.TryGetPropertyValue("id", out var idNode2) && idNode2?.GetValue<string>() == note.Id)))
                    {
                        arr.RemoveAt(i);
                    }
                }
            }

            // 写回已移除旧项的数组
            WriteArray(filePath, arr);

            // AppendNote 追加新项（AppendNote 内会再次读取并追加）
            AppendNote(filePath, note);
        }

        private static JsonArray LoadExistingArray(string filePath)
        {
            if (!File.Exists(filePath)) return new JsonArray();
            try
            {
                var text = File.ReadAllText(filePath).Trim();
                if (string.IsNullOrEmpty(text)) return new JsonArray();
                var parsed = JsonNode.Parse(text);
                return parsed as JsonArray ?? new JsonArray();
            }
            catch { return new JsonArray(); }
        }

        private static void WriteArray(string filePath, JsonArray arr)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            File.WriteAllText(filePath, arr.ToJsonString(options));
        }
    }
}