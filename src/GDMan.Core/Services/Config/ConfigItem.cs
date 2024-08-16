using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace GDMan.Core.Services.Config;

public partial class ConfigItem
{
    // public static readonly ConfigItem Summit = new(
    //     "enabled",
    //     "Whether or not the application should automatically check for updates",
    //     true.ToString(),
    //     (value) => bool.TryParse(value, out _),
    //     // bool.Parse,
    //     // value => value.ToString(),
    //     ConfigSection.UpdateCheck
    // );
    public string Name { get; }
    public string Description { get; }
    public string DefaultValue { get; }
    public Func<string, bool> ValidateString { get; }
    // public Func<string, T> FromString { get; }
    // public new Func<T, string> ToString { get; }
    public ConfigSection? Section { get; }

    private static readonly Dictionary<string, ConfigItem> KeyedConfigs = GetAllConfigs();
    public static readonly ReadOnlyCollection<ConfigItem> All = KeyedConfigs.Values.OrderBy(c => c.Section?.Name ?? "").ThenBy(x => x.Name).ToList().AsReadOnly();

    // public static readonly ConfigItem ShouldWork = new ConfigItem("YUP", "YUP", "YUP", (v) => true);

    static ConfigItem()
    {
        Console.WriteLine("ConfigItem static constructor");
    }

    private ConfigItem(string name, string description, string defaultValue, Func<string, bool> validateString, ConfigSection? section = null)
    {
        Name = name;
        Description = description;
        DefaultValue = defaultValue;
        ValidateString = validateString;
        Section = section;
        // FromString = fromString;
        // ToString = toString;
    }

    public static ConfigItem? GetFromString(string key)
        => KeyedConfigs[key];

    public string ToHelpString()
        => $"{Name} | {Description} | Defaults to ${DefaultValue}";

    public string ToFullyQualifiedString()
    {
        var sb = new StringBuilder();
        var parent = Section;

        while (parent != null)
        {
            sb.Append(parent.Name).Append('.');
        }
        return sb.Append(Name).ToString();
    }

    private static Dictionary<string, ConfigItem> GetAllConfigs()
    {
        var configs = new Dictionary<string, ConfigItem>();
        var fields = typeof(ConfigItem).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

        // var instance = new ConfigItem();
        foreach (var field in fields)
        {
            if (field.FieldType == typeof(ConfigItem))
            {
                var value = field.GetValue(null);
                var config = (ConfigItem?)value;
                if (config == null) continue;
                configs.Add(config.ToFullyQualifiedString(), config);
            }
        }

        return configs;
    }
}

// public partial class ConfigItem<T>
// {
//     public string Name { get; }
//     public string Description { get; }
//     public T DefaultValue { get; }
//     public Func<string, bool> ValidateString { get; }
//     public Func<string, T> FromString { get; }
//     public new Func<T, string> ToString { get; }
//     public ConfigSection? Section { get; }

//     protected static readonly Dictionary<string, ConfigItem> _items;

//     static ConfigItem()
//     {
//         _items = GetAllConfigs();
//     }

//     private ConfigItem(string name, string description, T defaultValue, Func<string, bool> validateString, Func<string, T> fromString, Func<T, string> toString, ConfigSection? section = null)
//     {
//         Name = name;
//         Description = description;
//         DefaultValue = defaultValue;
//         ValidateString = validateString;
//         Section = section;
//         FromString = fromString;
//         ToString = toString;
//     }

//     protected ConfigItem() { }

//     public string ToHelpString()
//         => $"{Name} | {Description} | Defaults to ${DefaultValue}";

//     public string ToFullyQualifiedString()
//     {
//         var sb = new StringBuilder();
//         var parent = Section;

//         while (parent != null)
//         {
//             sb.Append(parent.Name).Append('.');
//         }
//         return sb.Append(Name).ToString();
//     }

//     private static Dictionary<string, ConfigItem> GetAllConfigs()
//     {
//         var configs = new Dictionary<string, ConfigItem>();
//         var props = typeof(ConfigItem<>).GetProperties(BindingFlags.Static | BindingFlags.Public);

//         var instance = new ConfigItem();
//         foreach (var prop in props)
//         {
//             if (prop.PropertyType == typeof(ConfigItem<>))
//             {
//                 var value = prop.GetValue(instance);
//                 var config = (ConfigItem?)value;
//                 if (config == null) continue;
//                 configs.Add(config.ToFullyQualifiedString(), config);
//             }
//         }

//         return configs;
//     }
// }