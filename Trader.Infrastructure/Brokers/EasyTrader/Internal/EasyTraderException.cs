namespace Trader.Infrastructure.Brokers.EasyTrader.Internal
{
    internal class EasyTraderException : Exception
    {
        public int? HttpStatus { get; }
        public string? ResponseBody { get; }

        public EasyTraderException(string message)
            : base(message) { }

        public EasyTraderException(
            string message,
            int? httpStatus = null,
            string? responseBody = null)
            : base(message)
        {
            HttpStatus = httpStatus;
            ResponseBody = responseBody;
        }
    }
}