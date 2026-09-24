namespace Trader.Application.Dtos
{
    public record ServerClockInfoView(
        long Diff,
        long Offset,
        long OneWayLatency,
        long LastUpdatedAt,
        int SamplesCount,
        string? LastError
    );
}