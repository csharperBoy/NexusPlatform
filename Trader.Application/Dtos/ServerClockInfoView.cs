namespace Trader.Application.Dtos
{
    public class ServerClockInfoView
    {
        public long Diff { get; set; }
        public long Offset { get; set; }
        public long OneWayLatency { get; set; }
        public long LastUpdatedAt { get; set; }
        public int SamplesCount { get; set; }
        public string? LastError { get; set; }
    }
}