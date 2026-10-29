using Core.Domain.Common;
using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Abstractions;

public interface IInstrumentCommandService
{
    Task<Guid> CreateAsync(CreateInstrumentModel model);

    Task<bool> UpdateAsync(
        Guid id,
        Optional<string?> symbolName,
        Optional<string?> displayName,
        Optional<string?> boardCode,
        Optional<Guid?> tradingScheduleId,
        Optional<decimal> buyCommissionRate,
        Optional<decimal> sellCommissionRate,
        Optional<long> minOrderQuantity,
        Optional<long> maxOrderQuantity,
        Optional<long?> minOrderValueRial);

    Task DeactivateAsync(Guid id);
}