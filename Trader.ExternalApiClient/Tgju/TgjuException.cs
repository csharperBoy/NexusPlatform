namespace Trader.ExternalApiClient.Tgju;

public sealed class TgjuException : Exception
{
    public int? HttpStatus { get; }
    public string? ResponseBody { get; }

    public TgjuException(string message) : base(message) { }

    public TgjuException(
        string message, int? httpStatus, string? responseBody = null)
        : base(message)
    {
        HttpStatus = httpStatus;
        ResponseBody = responseBody;
    }

    public TgjuException(string message, Exception inner)
        : base(message, inner) { }
}