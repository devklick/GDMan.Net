using GDMan.Core.Helpers;
using GDMan.Core.Models;

namespace GDMan.Core.Services.Config;

public partial class ConfigItem
{
    public static readonly ConfigItem Target_Architecture = new(
        "architecture",
        "The system architecture to install versions of Godot for",
        ArchitectureHelper.FromSystem().ToString().ToLower(),
        (value) => ArchitectureHelper.TryParse(value, out _),
        // ArchitectureHelper.Parse,
        // (value) => value.ToString().ToLower(),
        ConfigSection.Target
    );

    public static readonly ConfigItem Target_Platform = new(
        "platform",
        "The OS platform to install versions of Godot for",
        PlatformHelper.FromSystem().ToString().ToLower(),
        (value) => PlatformHelper.TryParse(value, out _),
        // PlatformHelper.Parse,
        // value => value.ToString().ToLower(),
        ConfigSection.Target
    );

    public static readonly ConfigItem Target_Flavour = new(
        "flavour",
        "The flavour of Godot to install",
        Flavour.Standard.ToString().ToLower(),
        (value) => FlavourHelper.TryParse(value, out _),
        // FlavourHelper.Parse,
        // value => value.ToString().ToLower(),
        ConfigSection.UpdateCheck
    );
}