namespace Trader.Infrastructure.Brokers.EasyTrader.Internal
{
    internal class EasyTraderException : Exception
    {
        public int? HttpStatus { get; }
        public string? ResponseBody { get; }

        public EasyTraderException(string message)
            : base(message) { }

        public EasyTraderException(string message, Exception innerException)
            : base(message, innerException) { }

        public EasyTraderException(
            string message,
            int? httpStatus,
            string? responseBody = null)
            : base(message)
        {
            HttpStatus = httpStatus;
            ResponseBody = responseBody;
        }
    }
}