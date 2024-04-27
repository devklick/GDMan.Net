using System.Diagnostics.CodeAnalysis;

using GDMan.Cli.Attributes;

namespace GDMan.Cli.Options;

[Command("list", "l", "Lists the versions of Godot currently installed on the system")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public class ListOptions : BaseOptions, ICommandOptions
{
    public OptionValidation Validate()
        => new(true);
}