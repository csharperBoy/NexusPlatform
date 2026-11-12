using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Abstractions;

public interface IInstrumentOrderPresetService
{
    Task<IReadOnlyList<OrderPresetView>> GetListAsync();
    Task<OrderPresetView?> GetByIdAsync(Guid id);
    Task<OrderPresetView?> GetByInstrumentAsync(string isin);
    Task<Guid> CreateAsync(CreateOrderPresetModel model);
    Task UpdateAsync(Guid id, UpdateOrderPresetModel model);
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Resolve price + quantity برای این preset در لحظه.
    /// برای non-Fixed strategy از live API استفاده می‌کند؛
    /// اگر API در دسترس نبود (بازار بسته / API fail)، از ClosingPrice × range% fallback.
    /// </summary>
    Task<ResolvedOrderPresetView> ResolveAsync(Guid id, CancellationToken ct = default);
}