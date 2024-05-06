using GDMan.Cli.Attributes;
using GDMan.Core.Models;

namespace GDMan.Cli.Options;

[Command("uninstall", "u", "Uninstalls the specified version(s) of Godot")]
public class UninstallOptions : BaseOptions
{
    [Option("version", "v", "The version to use, e.g. 1.2.3. Any valid semver range is supported", OptionDataType.String)]
    public SemanticVersioning.Range? Version { get; set; }

    [Option("platform", "p", "The platform or operating system to find a version for", OptionDataType.Enum)]
    public Platform? Platform { get; set; }

    [Option("architecture", "a", "The system architecture to find a version for", OptionDataType.Enum)]
    public Architecture? Architecture { get; set; }

    [Option("flavour", "f", "The \"flavour\" (for lack of a better name) of version to use", OptionDataType.Enum)]
    public Flavour? Flavour { get; set; }

    [Option("flavour", "f", "The \"flavour\" (for lack of a better name) of version to use", OptionDataType.Enum)]
    public bool Force { get; set; } = false;

    [Option("unused", "u", "Install all versions other than the currently-active version", OptionDataType.Boolean, isFlag: true)]
    public bool Unused { get; set; } = false;

    public override OptionValidation Validate()
        => Version == null && !Unused
            ? OptionValidation.Failed("Either --version or --unused must be provided")
            : base.Validate();
}