namespace GDMan.Cli.Version;

public class CliVersionInfo
{
    public static readonly string FullName = "--version";
    public static readonly string ShortName = "-v";
    public static readonly string[] Names = [FullName, ShortName];

    public static SemanticVersioning.Version? AppVersion => GetVersion();
    private static SemanticVersioning.Version? GetVersion()
    {
        var version = typeof(CliVersionInfo).Assembly.GetName().Version;

        return version == null
            ? null
            : SemanticVersioning.Version.Parse($"{version.Major}.{version.Minor}.{version.Build}");
    }

    public new static string ToString()
        => AppVersion?.ToString() ?? "unknown version";
}