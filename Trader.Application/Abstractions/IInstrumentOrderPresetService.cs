using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Abstractions;

public interface IInstrumentOrderPresetService
{
    Task<Guid> CreateAsync(CreateOrderPresetModel model);
    Task UpdateAsync(Guid id, UpdateOrderPresetModel model);
    Task DeleteAsync(Guid id);
    Task<OrderPresetView?> GetByInstrumentAsync(string isin);
    Task SaveAsync();
}