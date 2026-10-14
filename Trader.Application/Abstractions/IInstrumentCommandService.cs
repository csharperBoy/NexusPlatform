using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Abstractions;

public interface IInstrumentCommandService
{
    Task<Guid> CreateAsync(CreateInstrumentModel model);
    Task UpdateAsync(Guid id, UpdateInstrumentModel model);
    Task DeactivateAsync(Guid id);
}