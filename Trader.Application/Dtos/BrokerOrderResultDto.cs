namespace Trader.Application.Dtos
{
    public class BrokerOrderResultDto
    {
        public bool IsSuccessful { get; set; }
        public string? OrderId { get; set; }
        public string? Message { get; set; }
        public int? ErrorCode { get; set; }
        public string? ErrorName { get; set; }
    }
}