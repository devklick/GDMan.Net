namespace GDMan.Cli.Args;

public class CliArgValidation(bool valid, object? value = null, params string[] messages)
{
    public object? Value { get; } = value;
    public bool Valid { get; } = valid;
    public IEnumerable<string> Messages { get; } = messages;

    public static CliArgValidation Failed(params string[] messages)
        => new(false, null, messages);

    public static CliArgValidation Success(object? value = null)
        => new(true, value);
}