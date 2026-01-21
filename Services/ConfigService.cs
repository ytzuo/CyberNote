using System;
using System.Diagnostics;
using System.IO;

namespace CyberNote.Services
{
    public static class ConfigService
    {
        private const string Company = "CyberNote";
        private const string Product = "CyberNote";
        private const string ConfigFileName = "config_path.txt";

        // 存储在 AppData\Roaming\CyberNote\CyberNote\config_path.txt
        private static readonly string AppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Company, Product);
        private static readonly string ConfigFilePath = Path.Combine(AppDataDir, ConfigFileName);

        private static string? _dataFilePathCache;
        private static string? _recordFilePathCache;

        // 私有方法：读取配置（兼容旧格式）
        private static (string notePath, string recordPath) LoadConfigInternal()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    var content = File.ReadAllText(ConfigFilePath).Trim();
                    if (string.IsNullOrWhiteSpace(content)) return (null, null);

                    // 尝试作为JSON解析
                    if (content.StartsWith("{") && content.EndsWith("}"))
                    {
                        try
                        {
                            var json = System.Text.Json.Nodes.JsonNode.Parse(content);
                            var nNode = json?["NoteDataPath"];
                            var rNode = json?["RecordDataPath"];
                            
                            var nPath = nNode?.GetValue<string>();
                            var rPath = rNode?.GetValue<string>();
                            return (nPath, rPath);
                        }
                        catch { }
                    }
                    
                    // 否则视为旧格式（纯路径字符串）
                    return (content, null);   
                }
            }
            catch { }
            return (null, null);
        }

        // 私有方法：保存配置
        private static void SaveConfigInternal(string? notePath, string? recordPath)
        {
            try
            {
                var nPath = notePath;
                if (string.IsNullOrWhiteSpace(nPath))
                {
                    // 如果没传notePath，尝试读取现有的
                    // 这里的逻辑稍微简化，假定setter调用时会传入正确的值，或者getter初始化时会补全
                    nPath = Path.Combine(AppDataDir, "data.json"); 
                }
                
                var rPath = recordPath;
                if (string.IsNullOrWhiteSpace(rPath))
                {
                    rPath = Path.Combine(AppDataDir, "records.json");
                }

                var obj = new System.Text.Json.Nodes.JsonObject
                {
                    ["NoteDataPath"] = nPath,
                    ["RecordDataPath"] = rPath
                };
                
                Directory.CreateDirectory(AppDataDir);
                File.WriteAllText(ConfigFilePath, obj.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }

        public static string DataFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataFilePathCache)) return _dataFilePathCache!;
                
                var (nPath, _) = LoadConfigInternal();
                if (!string.IsNullOrWhiteSpace(nPath))
                {
                    _dataFilePathCache = nPath;
                    EnsureFileExists(_dataFilePathCache);
                    return _dataFilePathCache!;
                }

                // 默认逻辑
                var defaultPath = Path.Combine(AppDataDir, "data.json");
                _dataFilePathCache = defaultPath;
                
                // 只有当缓存为空时，才去触发一次保存（初始化默认值）
                // 此时我们需要获取 Record 的当前值或默认值一起保存
                var (_, rPath) = LoadConfigInternal(); // 重新读不太高效但安全
                SaveConfigInternal(defaultPath, rPath); 
                
                EnsureFileExists(_dataFilePathCache);
                return defaultPath;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                _dataFilePathCache = value;
                // 获取当前的 RecordPath 以便一起保存
                var rPath = RecordFilePath; // 通过 getter 获取（带缓存或读取）
                SaveConfigInternal(value, rPath);
            }
        }

        public static string RecordFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_recordFilePathCache)) return _recordFilePathCache!;

                var (_, rPath) = LoadConfigInternal();
                if (!string.IsNullOrWhiteSpace(rPath))
                {
                    _recordFilePathCache = rPath;
                    EnsureFileExists(_recordFilePathCache);
                    return _recordFilePathCache!;
                }

                var defaultPath = Path.Combine(AppDataDir, "records.json");
                _recordFilePathCache = defaultPath;

                var nPath = DataFilePath; // 获取当前的 NotePath
                SaveConfigInternal(nPath, defaultPath);

                EnsureFileExists(_recordFilePathCache);
                return defaultPath;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                _recordFilePathCache = value;
                
                var nPath = DataFilePath;
                SaveConfigInternal(nPath, value);
            }
        }

        private static void EnsureFileExists(string path)
        {
            if (!File.Exists(path))
            {
                try 
                {
                    var dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(path, "[]"); 
                } 
                catch 
                {
                    Debug.WriteLine($"Failed to create file at {path}");
                }
            }
        }
    }
}
