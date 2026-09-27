namespace Trader.Application.Abstractions
{
    public interface ISymbolCommandService
    {
        Task<Guid> CreateSymbolAsync(
            string symbolName,
            string symbolIsin,
            long price,
            long quantity,
            int side,
            int validityType,
            decimal commission,
            int orderModelType,
            int orderFrom);

        Task<Guid> UpdateSymbolAsync(
            Guid id,
            string symbolName,
            string symbolIsin,
            long price,
            long quantity,
            int side,
            int validityType,
            decimal commission,
            int orderModelType,
            int orderFrom);

        Task<bool> DeleteSymbolAsync(Guid id);

        Task SaveAsync();
    }
}