using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Infrastructure.Services;

namespace Trader.Infrastructure.Scheduler
{
    public class ClockSyncBackgroundService : BackgroundService
    {
        private readonly ServerClockService _clockService;
        private readonly ClockSyncOptions _options;
        private readonly TimeZoneInfo _tz;
        private readonly ILogger<ClockSyncBackgroundService> _logger;

        public ClockSyncBackgroundService(
            ServerClockService clockService,
            IOptions<ClockSyncOptions> options,
            ILogger<ClockSyncBackgroundService> logger)
        {
            _clockService = clockService;
            _options = options.Value;
            _logger = logger;

            try
            {
                _tz = TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZone);
            }
            catch
            {
                _logger.LogWarning(
                    "TimeZone '{Tz}' not found, using UTC", _options.TimeZone);
                _tz = TimeZoneInfo.Utc;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var parsedTimes = _options.SyncTimes
                .Select(t => TimeOnly.TryParse(t, out var to) ? to : (TimeOnly?)null)
                .Where(t => t.HasValue)
                .Select(t => t!.Value)
                .OrderBy(t => t)
                .ToList();

            if (parsedTimes.Count == 0)
            {
                _logger.LogWarning("No valid sync times configured");
                return;
            }

            _logger.LogInformation(
                "Clock auto-sync enabled: {Times} (tz={Tz})",
                string.Join(", ", parsedTimes), _options.TimeZone);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var nextUtc = ComputeNextSyncUtc(parsedTimes);
                    var delay = nextUtc - DateTimeOffset.UtcNow;

                    _logger.LogInformation(
                        "Next clock sync at {Time} (in {Delay})",
                        TimeZoneInfo.ConvertTime(nextUtc, _tz), delay);

                    if (delay > TimeSpan.Zero)
                        await Task.Delay(delay, ct);

                    if (ct.IsCancellationRequested) break;

                    _logger.LogInformation("Auto-syncing clock");
                    var r = await _clockService.SyncAsync(_options.SamplesPerSync);
                    _logger.LogInformation(
                        "Auto-sync done: diff={Diff}ms samples={N}",
                        r.Diff, r.SamplesCount);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Auto-sync failed, retry in 30s");
                    await Task.Delay(TimeSpan.FromSeconds(30), ct);
                }
            }
        }

        private DateTimeOffset ComputeNextSyncUtc(List<TimeOnly> times)
        {
            var nowUtc = DateTimeOffset.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTime(nowUtc, _tz);
            var todayLocal = nowLocal.Date;

            foreach (var t in times)
            {
                var dt = todayLocal.Add(t.ToTimeSpan());
                var candidate = new DateTimeOffset(dt, _tz.GetUtcOffset(dt));
                if (candidate > nowUtc) return candidate;
            }

            var tomorrowLocal = todayLocal.AddDays(1);
            var firstDt = tomorrowLocal.Add(times[0].ToTimeSpan());
            return new DateTimeOffset(firstDt, _tz.GetUtcOffset(firstDt));
        }
    }
}