using System;
using System.IO;
using System.Text.Json;

namespace RestaurantSystem.Data;

public class DatabaseConfig
{
    private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconfig.json");
    private static DatabaseConfig? _instance;

    public string Provider { get; set; } = "SQLite";
    public string Mode { get; set; } = "Local"; // "Local" or "Network"
    public string SqlitePath { get; set; } = "restaurant.db";

    public static DatabaseConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Load();
            }
            return _instance;
        }
    }

    public static string GetEffectiveSqlitePath()
    {
        var config = Instance;
        string path = config.SqlitePath.Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            path = "restaurant.db";
        }

        // If it's a UNC path (\\server\share\...) or rooted path (C:\...)
        if (path.StartsWith(@"\\") || Path.IsPathRooted(path))
        {
            return path;
        }

        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
    }

    public static string GetEffectiveConnectionString()
    {
        return $"Data Source={GetEffectiveSqlitePath()}";
    }

    public static DatabaseConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                var loaded = JsonSerializer.Deserialize<DatabaseConfig>(json);
                if (loaded != null)
                {
                    return loaded;
                }
            }
        }
        catch { }

        var defaultCfg = new DatabaseConfig();
        defaultCfg.Save();
        return defaultCfg;
    }

    public void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(ConfigPath, json);
            _instance = this;
        }
        catch { }
    }
}
