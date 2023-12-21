namespace GDMan.Core.Models;

public class Result<TValue>
{
    public ResultStatus Status { get; set; }
    public TValue? Value { get; set; }
    public List<string> Messages { get; set; } = [];

    public Result() { }

    public Result(ResultStatus status, TValue? value, params string[] messages)
    {
        Status = status;
        Value = value;
        Messages = [.. messages];
    }
}