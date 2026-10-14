using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Abstractions;

public interface IInstrumentQueryService
{
    Task<IReadOnlyList<InstrumentInfoView>> GetListAsync();
    Task<InstrumentInfoView?> GetByIsinAsync(string isin);
    Task<IReadOnlyList<InstrumentSelectionView>> GetSelectionListAsync();
}