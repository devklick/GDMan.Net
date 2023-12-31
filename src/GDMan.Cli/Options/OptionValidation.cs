namespace GDMan.Cli.Options;

public class OptionValidation(bool valid, object? value = null, params string[] messages)
{
    public object? Value { get; } = value;
    public bool Valid { get; } = valid;
    public IEnumerable<string> Messages { get; } = messages;

    public static OptionValidation Failed(params string[] messages)
        => new(false, null, messages);

    public static OptionValidation Success(object? value = null)
        => new(true, value);
}