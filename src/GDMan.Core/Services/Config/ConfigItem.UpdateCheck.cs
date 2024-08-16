namespace GDMan.Core.Services.Config;

public partial class ConfigItem
{
    public static readonly ConfigItem UpdateCheck_Enabled = new(
        "enabled",
        "Whether or not the application should automatically check for updates",
        true.ToString(),
        (value) => bool.TryParse(value, out _),
        // bool.Parse,
        // value => value.ToString(),
        ConfigSection.UpdateCheck
    );

    public static readonly ConfigItem UpdateCheck_FrequencyInDays = new(
        "frequencyInDays",
        "The number of days to wait between automatically checking for updates",
        7.ToString(),
        (value) => int.TryParse(value, out _),
        // int.Parse,
        // value => value.ToString(),
        ConfigSection.UpdateCheck
    );

    public static readonly ConfigItem UpdateCheck_LastCheckDate = new(
        "lastCheckDate",
        "The date that the application last checked for updated",
        DateTime.MinValue.ToString(),
        (value) => DateTime.TryParse(value, out _),
        // DateTime.Parse,
        // value => value.ToString(),
        ConfigSection.UpdateCheck
    );
}