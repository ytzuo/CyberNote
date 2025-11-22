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