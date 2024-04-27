using System.Diagnostics.CodeAnalysis;

using GDMan.Cli.Attributes;

namespace GDMan.Cli.Options;

[Command("current", "c", "Prints the currently-active version of Godot")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public class CurrentOptions : BaseOptions, ICommandOptions
{
    public OptionValidation Validate()
        => new(true);
}