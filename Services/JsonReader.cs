using System;
using System.IO;
using System.Text.Json;

namespace CyberNote.Services
{
    public static class JsonReader
    {
        public static string GetNote(string filePath, string key, string target)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("找不到 JSON 文件", filePath);

            using var doc = JsonDocument.Parse(File.ReadAllText(filePath));
            var root = doc.RootElement;

            // 如果根是数组：按每个对象的 "Title" 精确匹配 key，返回对应的 target的内容"Content"

            foreach (var item in root.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;

                if (item.TryGetProperty("Title", out var title) &&
                    string.Equals(title.GetString(), key, StringComparison.Ordinal))
                {
                    if (item.TryGetProperty(target, out var content))
                        return content.ToString();
                }
                return null;

            }
            return null;
        }
        public static List<(string Content, bool Progress)>? GetTask(string filePath, string key)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(filePath));
                JsonElement root = doc.RootElement;

                // root 是个数组，先找到匹配 key 的节点
                foreach (JsonElement node in root.EnumerateArray())
                {
                    if (!node.TryGetProperty("Title", out JsonElement titleEl) ||
                        !titleEl.ValueEquals(key))
                        continue;

                    // 确认是 List 类型
                    if (!node.TryGetProperty("Type", out JsonElement typeEl) ||
                        !typeEl.ValueEquals("List"))
                        return null;          // 不是 List 类型

                    if (!node.TryGetProperty("Tasks", out JsonElement tasksEl) ||
                        tasksEl.ValueKind != JsonValueKind.Array)
                        return new();         // 没有 Tasks 字段，或为空数组

                    var list = new List<(string, bool)>();
                    foreach (JsonElement task in tasksEl.EnumerateArray())
                    {
                        string content = task.GetProperty("Content").GetString() ?? string.Empty;
                        bool progress = task.GetProperty("Progress").GetBoolean();
                        list.Add((content, progress));
                    }
                    return list;
                }
            }
            catch { /* 文件不存在或解析失败 */ }
            return null;  // 没找到对应 key
            }
        }
    }