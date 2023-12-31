using System.Diagnostics.CodeAnalysis;

using GDMan.Cli.Args;

namespace GDMan.Cli.Options;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public interface ICommandOptions
{
    public OptionValidation Validate();
}