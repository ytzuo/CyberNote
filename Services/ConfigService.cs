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
                            return path;
                        }
                    }
                }
                catch { }
                // 默认路径（相对于项目根目录的 Data\data.json）
                var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var projectRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(assemblyLocation)));
                var defaultPath = Path.Combine(projectRoot ?? AppContext.BaseDirectory, "Data", "data.json");

                // 缓存并在 AppData 中持久化默认路径，保证首次运行后也能找到配置
                _dataFilePathCache = defaultPath;
                try
                {
                    Directory.CreateDirectory(AppDataDir);
                    File.WriteAllText(ConfigFilePath, defaultPath);
                }
                catch { /* 忽略持久化异常 */ }

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
