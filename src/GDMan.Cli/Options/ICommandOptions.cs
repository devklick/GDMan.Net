using System.Diagnostics.CodeAnalysis;

namespace GDMan.Cli.Options;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public interface ICommandOptions
{
    public OptionValidation Validate();
}