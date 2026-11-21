namespace Trader.ExternalApiClient.Tsetmc;

public sealed class TsetmcException : Exception
{
    public int? HttpStatus { get; }
    public string? ResponseBody { get; }

    public TsetmcException(string message) : base(message) { }

    public TsetmcException(string message, Exception inner)
        : base(message, inner) { }

    public TsetmcException(
        string message,
        int? httpStatus,
        string? responseBody = null)
        : base(message)
    {
        HttpStatus = httpStatus;
        ResponseBody = responseBody;
    }
}