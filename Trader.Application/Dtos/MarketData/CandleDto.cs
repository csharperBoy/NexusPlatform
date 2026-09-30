namespace Trader.Application.Dtos.MarketData
{
    /// <summary>
    /// کندل OHLCV
    /// </summary>
    public class CandleDto
    {
        /// <summary>Unix seconds — ابتدای دوره</summary>
        public long T { get; set; }
        public decimal O { get; set; }
        public decimal H { get; set; }
        public decimal L { get; set; }
        public decimal C { get; set; }
        public long V { get; set; }
    }

    public class CandleHistoryDto
    {
        public List<CandleDto> Data { get; set; } = new();
        public string S { get; set; } = default!;
    }
}