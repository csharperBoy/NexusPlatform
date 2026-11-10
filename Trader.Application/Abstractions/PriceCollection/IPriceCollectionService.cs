using Trader.Application.Dtos.PriceCollection;

namespace Trader.Application.Abstractions.PriceCollection;

public interface IPriceCollectionService
{
    // ─── PriceSource CRUD ───
    Task<Guid> CreateSourceAsync(CreatePriceSourceModel model);
    Task UpdateSourceAsync(Guid id, CreatePriceSourceModel model);
    Task DeactivateSourceAsync(Guid id);
    Task SaveAsync();

    // ─── PriceCollectionJob ───
    Task<Guid> CreateJobAsync(CreatePriceCollectionJobModel model);
    Task StartJobAsync(Guid id);
    Task StopJobAsync(Guid id);

    // ─── Collection (called from Handler) ───
    Task CollectOnceAsync(Guid jobId, CancellationToken ct = default);
    Task CollectBackfillAsync(Guid jobId, CancellationToken ct = default);
}