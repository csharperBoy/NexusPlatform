using Core.Domain.Common;
using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Abstractions;

public interface IInstrumentCommandService
{
    Task<Guid> CreateAsync(CreateInstrumentModel model);

    /// <summary>
    /// فقط فیلدهایی که در Command ست شده‌اند (IsSet=true) اعمال می‌شوند.
    /// خروجی: آیا تغییری اعمال شد.
    /// </summary>
    Task<bool> UpdateAsync(
        Guid id,
        Optional<string?> symbolName,
        Optional<string?> displayName,
        Optional<string?> boardCode,
        Optional<Guid?> tradingScheduleId);

    Task DeactivateAsync(Guid id);
}