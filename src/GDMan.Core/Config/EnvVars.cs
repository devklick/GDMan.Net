namespace GDMan.Core.Config;

public class EnvVar(string name)
{
    public string Name { get; } = name;
    public string? Value { get; } = Environment.GetEnvironmentVariable(name);
}

public static class EnvVars
{
    public static readonly EnvVar TargetArchitecture = new("GDMAN_TARGET_ARCHITECTURE");
    public static readonly EnvVar TargetPlatform = new("GDMAN_TARGET_PLATFORM");
    public static readonly EnvVar TargetFlavour = new("GDMAN_TARGET_FLAVOUR");
    public static readonly EnvVar VersionsDir = new("GDMAN_VERSIONS_DIR");
}