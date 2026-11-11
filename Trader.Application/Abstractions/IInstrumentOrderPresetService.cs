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
}