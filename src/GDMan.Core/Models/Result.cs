namespace GDMan.Core.Models;

public class Result<TValue> : ApiResult<TValue, List<string>>
{
    public Result() { }

    public Result(ResultStatus status, TValue? value, params string[] errors)
        : base(status, value, errors.ToList())
    { }
}

public class ApiResult<TValue, TError>
{
    public ResultStatus Status { get; set; }
    public TValue? Value { get; set; }
    public TError? Error { get; set; }
    public List<string> Messages { get; set; } = [];

    public ApiResult() { }

    public ApiResult(ResultStatus status, TValue? value, TError? error)
    {
        Status = status;
        Value = value;
        Error = error;
    }
}