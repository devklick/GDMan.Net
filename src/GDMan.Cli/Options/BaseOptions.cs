using System.Diagnostics.CodeAnalysis;

using GDMan.Cli.Attributes;

namespace GDMan.Cli.Options;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)]
public class BaseOptions
{
    [Option("verbose", "vl", "Whether or not extensive information should be logged", OptionDataType.Boolean, isFlag: true)]
    public bool Verbose { get; set; } = false;

    [Option("help", "h", "Prints the help information to the console", OptionDataType.Boolean, isFlag: true)]
    public bool Help { get; set; } = false;

    public virtual OptionValidation Validate()
        => new(true);
}