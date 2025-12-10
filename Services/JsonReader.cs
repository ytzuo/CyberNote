using CyberNote.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Linq;
using System.Windows; // MessageBox
using Microsoft.Win32; // OpenFileDialog

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
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(filePath));
                var list = new List<NoteCard>();
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    if (el.ValueKind != JsonValueKind.Object) continue;
                    if (!el.TryGetProperty("Type", out var typeProp)) continue;
                    var type = typeProp.GetString();

                    // 读取 id （允许旧数据无 id）
                    string id = el.TryGetProperty("id", out var idProp) ? (idProp.GetString() ?? Guid.NewGuid().ToString()) : Guid.NewGuid().ToString();

                    switch (type)
                    {
                        case "Common":
                            var common = JsonSerializer.Deserialize<CommonNote>(el.GetRawText());
                            if (common != null)
                            {
                                if (string.IsNullOrWhiteSpace(common.Id)) common.Id = id;
                                list.Add(common);
                            }
                            break;
                        case "List":
                            var listNote = JsonSerializer.Deserialize<ListNote>(el.GetRawText());
                            if (listNote != null)
                            {
                                if (string.IsNullOrWhiteSpace(listNote.Id)) listNote.Id = id;
                                foreach (var t in listNote.Tasks)
                                    t.Owner = listNote;
                                list.Add(listNote);
                            }
                            break;
                    }
                }
                return list;
            }
            catch (Exception)
            {
                // JSON 解析失败：提示并让用户选择新的 JSON 文件
                System.Windows.MessageBox.Show("JSON 文件无效或已损坏，请选择一个有效的 JSON 文件。", "读取失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "选择便签数据 JSON 文件",
                    Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                    CheckFileExists = true,
                    Multiselect = false
                };
                if (dlg.ShowDialog() == true)
                {
                    // 更新全局路径并重试读取
                    ConfigService.DataFilePath = dlg.FileName;
                    if (File.Exists(dlg.FileName))
                    {
                        return LoadAllCard(dlg.FileName);
                    }
                }
                return new List<NoteCard>();
            }
        }
    }
}

