namespace GDMan.Cli.Args;

public class CliArgValidation(bool valid, object? value = null)
{
    public object? Value { get; } = value;
    public bool Valid { get; } = valid;

    public static CliArgValidation Failed()
        => new(false);

    public static CliArgValidation Success(object value)
        => new(true, value);
}