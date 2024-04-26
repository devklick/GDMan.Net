using GDMan.Cli.Attributes;

namespace GDMan.Cli.Options;

public class BaseOptions
{
    [Option("verbose", "vl", "Whether or not extensive information should be logged", OptionDataType.Boolean, isFlag: true)]
    public bool Verbose { get; set; } = false;

    [Option("help", "h", "Prints the help information to the console", OptionDataType.Boolean, isFlag: true)]
    public bool Help { get; set; } = false;
}