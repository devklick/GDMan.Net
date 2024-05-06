using GDMan.Core.Config;
using GDMan.Core.Models;

namespace GDMan.Core.Helpers;

public class FlavourHelper : EnumHelper
{
    public static Flavour? FromEnvVar()
    {
        var envVar = EnvVars.TargetFlavour.Value;

        if (string.IsNullOrEmpty(envVar)) return null;

        if (TryParse<Flavour>(envVar, out var flavour)) return flavour;

        throw new FormatException($"Invalid value for {EnvVars.TargetFlavour.Name}");
    }
}