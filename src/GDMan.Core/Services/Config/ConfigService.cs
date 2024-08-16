using GDMan.Core.Services.FileSystem;

namespace GDMan.Core.Services.Config;

public class ConfigService
{
    static ConfigService()
    {
        if (!File.Exists(KnownPaths.ConfigFilePath))
        {
            InitConfigFile();
        }
    }

    public static async Task SetValue(string key, string value) { }
    public static async Task<string> GetValue(string key)
    {
        throw new Exception();
    }

    private static async Task WriteConfigs()
    {

    }

    private static void InitConfigFile()
    {
        var lines = new List<string>();
        ConfigSection? currentSection = null;
        foreach (var config in ConfigItem.All)
        {
            var t = "";
            if (config.Section != null)
            {
                t = "\t";
                if (config.Section != currentSection)
                {
                    lines.Add(Environment.NewLine);
                    lines.Add($"[{config.Section.Name}]");
                }
            }
            lines.Add($"{t}# {config.Description}");
            lines.Add($"{t}{config.Name} = {config.DefaultValue}");
        }
        File.WriteAllLines(KnownPaths.ConfigFilePath, lines);
    }
}