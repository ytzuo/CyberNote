using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using CyberNote.Models;

namespace CyberNote.Services
{
    public static class JsonWriter
    {
        public static void AppendNote(string filePath, NoteCard note)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));

            // 读取现有 JSON 数组；不存在则新建
            JsonArray rootArray;
            if (File.Exists(filePath))
            {
                try
                {
                    var text = File.ReadAllText(filePath).Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        var parsed = JsonNode.Parse(text);
                        rootArray = parsed as JsonArray ?? new JsonArray();
                    }
                    else rootArray = new JsonArray();
                }
                catch
                {
                    // 文件损坏时，重建数组
                    rootArray = new JsonArray();
                }
            }
            else
            {
                rootArray = new JsonArray();
            }

            // 通过多态接口生成 JsonObject 并追加
            var obj = note.toJson();
            rootArray.Add(obj);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
            };

            File.WriteAllText(filePath, rootArray.ToJsonString(options));
        }
    }
}