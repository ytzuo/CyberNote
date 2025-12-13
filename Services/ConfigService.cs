using System;
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

        public static string DataFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataFilePathCache)) return _dataFilePathCache!;
                try
                {
                    if (File.Exists(ConfigFilePath))
                    {
                        var path = File.ReadAllText(ConfigFilePath).Trim();
                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            _dataFilePathCache = path;
                            // 确保数据文件存在，如果不存在则创建空JSON数组
                            if (!File.Exists(_dataFilePathCache))
                            {
                                try { File.WriteAllText(_dataFilePathCache, "[]"); } catch { }
                            }
                            return path;
                        }
                    }
                }
                catch { }
                // 默认路径（与config_path.txt同目录）
                var defaultPath = Path.Combine(AppDataDir, "data.json");

                // 缓存并在 AppData 中持久化默认路径，保证首次运行后也能找到配置
                _dataFilePathCache = defaultPath;
                try
                {
                    Directory.CreateDirectory(AppDataDir);
                    File.WriteAllText(ConfigFilePath, defaultPath);
                }
                catch { /* 忽略持久化异常 */ }

                // 确保数据文件存在，如果不存在则创建空JSON数组
                if (!File.Exists(_dataFilePathCache))
                {
                    try { File.WriteAllText(_dataFilePathCache, "[]"); } catch { }
                }

                return defaultPath;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                _dataFilePathCache = value;
                try
                {
                    Directory.CreateDirectory(AppDataDir);
                    File.WriteAllText(ConfigFilePath, value);
                }
                catch { /* 忽略持久化异常 */ }
            }
        }
    }
}
