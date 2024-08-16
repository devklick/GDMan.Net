using System.Diagnostics.CodeAnalysis;

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

    public static bool TryParse(string value, [NotNullWhen(true)] out Flavour? enumMember)
        => EnumHelper.TryParse(value, out enumMember);

    public static Flavour Parse(string value)
        => TryParse(value, out var flavour)
            ? flavour.Value
            : throw new FormatException($"Invalid value for Flavour: {value}");
}