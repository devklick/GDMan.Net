using System.Collections.ObjectModel;
using System.Reflection;

namespace GDMan.Core.Services.Config;

public class ConfigSection
{
    public static readonly ConfigSection UpdateCheck = new("updateCheck");
    public static readonly ConfigSection Target = new("target");
    // public static readonly ReadOnlyCollection<ConfigSection> All = GetConfigSections();
    public string Name { get; }
    public ReadOnlyCollection<ConfigItem> Configs { get; }

    private ConfigSection(string name, ConfigSection? parent = null)
    {
        Name = name;
        Configs = ConfigItem.All.Where(c => c.Section == this).ToList().AsReadOnly();
    }

    // private static ReadOnlyCollection<ConfigSection> GetConfigSections()
    // {
    //     var sections = new List<ConfigSection>();
    //     var props = typeof(ConfigSection).GetProperties(BindingFlags.Public | BindingFlags.Static);
    //     foreach (var prop in props)
    //     {
    //         if (prop.PropertyType == typeof(ConfigSection))
    //         {
    //             var section = (ConfigSection?)prop.GetValue(null);
    //             if (section != null)
    //             {
    //                 sections.Add(section);
    //             }
    //         }
    //     }

    //     return sections.AsReadOnly();
    // }
}