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
        private static int? _themeIndexCache;
        private static bool? _isDarkModeCache;

        // 私有方法：读取配置（兼容旧格式）
        private static (string? notePath, string? recordPath, int themeIndex, bool isDarkMode) LoadConfigInternal()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    var content = File.ReadAllText(ConfigFilePath).Trim();
                    if (string.IsNullOrWhiteSpace(content)) return (null, null, 0, false);

                    // 尝试作为JSON解析
                    if (content.StartsWith("{") && content.EndsWith("}"))
                    {
                        try
                        {
                            var json = System.Text.Json.Nodes.JsonNode.Parse(content);
                            var nNode = json?["NoteDataPath"];
                            var rNode = json?["RecordDataPath"];
                            var tNode = json?["ThemeIndex"];
                            var dNode = json?["IsDarkMode"];
                            
                            var nPath = nNode?.GetValue<string>();
                            var rPath = rNode?.GetValue<string>();
                            var tIdx = tNode?.GetValue<int>() ?? 0;
                            var isDark = dNode?.GetValue<bool>() ?? false;
                            return (nPath, rPath, tIdx, isDark);
                        }
                        catch { }
                    }
                    
                    // 否则视为旧格式（纯路径字符串）
                    return (content, null, 0, false);   
                }
            }
            catch { }
            return (null, null, 0, false);
        }

        // 私有方法：保存配置
        private static void SaveConfigInternal(string? notePath, string? recordPath, int? themeIndex, bool? isDarkMode)
        {
            try
            {
                var nPath = notePath ?? DataFilePath; // Use property to ensure default if null
                var rPath = recordPath ?? RecordFilePath;
                var tIdx = themeIndex ?? ThemeIndex; // Use property
                var isDark = isDarkMode ?? IsDarkMode;

                var obj = new System.Text.Json.Nodes.JsonObject
                {
                    ["NoteDataPath"] = nPath,
                    ["RecordDataPath"] = rPath,
                    ["ThemeIndex"] = tIdx,
                    ["IsDarkMode"] = isDark
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
                
                var (nPath, _, _, _) = LoadConfigInternal();
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
                SaveConfigInternal(defaultPath, null, null, null); 
                
                EnsureFileExists(_dataFilePathCache);
                return defaultPath;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                _dataFilePathCache = value;
                SaveConfigInternal(value, null, null, null);
            }
        }

        public static string RecordFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_recordFilePathCache)) return _recordFilePathCache!;

                var (_, rPath, _, _) = LoadConfigInternal();
                if (!string.IsNullOrWhiteSpace(rPath))
                {
                    _recordFilePathCache = rPath;
                    EnsureFileExists(_recordFilePathCache);
                    return _recordFilePathCache!;
                }

                var defaultPath = Path.Combine(AppDataDir, "records.json");
                _recordFilePathCache = defaultPath;

                SaveConfigInternal(null, defaultPath, null, null);

                EnsureFileExists(_recordFilePathCache);
                return defaultPath;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                _recordFilePathCache = value;
                SaveConfigInternal(null, value, null, null);
            }
        }

        public static int ThemeIndex
        {
            get
            {
                if (_themeIndexCache.HasValue) return _themeIndexCache.Value;
                var (_, _, tIdx, _) = LoadConfigInternal();
                _themeIndexCache = tIdx;
                return tIdx;
            }
            set
            {
                _themeIndexCache = value;
                SaveConfigInternal(null, null, value, null);
            }
        }

        public static bool IsDarkMode
        {
            get
            {
                if (_isDarkModeCache.HasValue) return _isDarkModeCache.Value;
                var (_, _, _, isDark) = LoadConfigInternal();
                _isDarkModeCache = isDark;
                return isDark;
            }
            set
            {
                _isDarkModeCache = value;
                SaveConfigInternal(null, null, null, value);
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
