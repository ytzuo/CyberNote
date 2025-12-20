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
        // 全局并发保护, 确保文件写入的线程安全
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
        public static async Task AppendNoteAsync(string filePath, NoteCard note)
        {
            await _fileLock.WaitAsync();
            try
            {
                var arr = LoadExistingArray(filePath); // 可以改成异步读实现，但同步读也可以（在锁内）
                                                       // 检查重复 ID...
                arr.Add(note.toJson());
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                await WriteArrayAtomicAsync(filePath, arr, options);
            }
            finally { _fileLock.Release(); }
        }

        //实现保存修改（替换或追加）
        public static async Task SaveNote(string filePath, NoteCard note)
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
            await WriteArrayAsync(filePath, arr);

            // AppendNote 追加新项（AppendNote 内会再次读取并追加）
            await AppendNoteAsync(filePath, note);
        }

        // 删除指定的记录
        public static async Task DeleteNote(string filePath, string noteId)
        {
            if (string.IsNullOrWhiteSpace(noteId)) throw new ArgumentNullException(nameof(noteId));
            var arr = LoadExistingArray(filePath);
            bool removed = false;
            for (int i = arr.Count - 1; i >= 0; i--)
            {
                if (arr[i] is JsonObject o &&
                    ((o.TryGetPropertyValue("Id", out var idNode) && idNode?.GetValue<string>() == noteId) ||
                     (o.TryGetPropertyValue("id", out var idNode2) && idNode2?.GetValue<string>() == noteId)))
                {
                    arr.RemoveAt(i);
                    removed = true;
                }
            }
            if (removed)
            {
                await WriteArrayAsync(filePath, arr);
            }
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

        private static async Task WriteArrayAsync(string filePath, JsonArray arr)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var text = arr.ToJsonString(options);
            var dir = Path.GetDirectoryName(filePath) ?? ".";
            Directory.CreateDirectory(dir);
            await File.WriteAllTextAsync(filePath, text);
        }

        public static async Task WriteArrayAtomicAsync(string filePath, JsonArray arr, JsonSerializerOptions options)
        {
            var dir = Path.GetDirectoryName(filePath) ?? ".";
            Directory.CreateDirectory(dir);

            // 临时文件（同一目录）
            var tempPath = Path.Combine(dir, Path.GetFileName(filePath) + ".tmp." + Guid.NewGuid().ToString("N"));
            var backupPath = Path.Combine(dir, Path.GetFileName(filePath) + ".bak");

            await _fileLock.WaitAsync();
            try
            {
                // 使用异步 FileStream 写入（useAsync: true）并序列化到流
                using (var fs = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await JsonSerializer.SerializeAsync(fs, arr, options);
                    await fs.FlushAsync(); // 把内容刷新到操作系统缓存
                }

                // 原子替换：File.Replace 会用临时文件替换目标并可创建备份（Windows）
                if (File.Exists(filePath))
                {
                    // backupPath 可为 null；若不想保留备份可改用 File.Move(tempPath, filePath, true)（.NET 6+）
                    File.Replace(tempPath, filePath, backupPath, ignoreMetadataErrors: true);
                    // 删除备份（可选）
                    if (File.Exists(backupPath)) File.Delete(backupPath);
                }
                else
                {
                    // 目标不存在，直接移动临时文件为目标（在同一分区是原子操作）
                    File.Move(tempPath, filePath);
                }
            }
            catch
            {
                // 失败时尝试清理临时文件
                try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }
    }
}