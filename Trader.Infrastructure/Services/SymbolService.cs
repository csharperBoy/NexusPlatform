using Core.Application.Abstractions;
using Core.Application.Abstractions.Security;
using Core.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services
{
    public class SymbolService : ISymbolCommandService, ISymbolQueryService
    {
        private readonly IRepository<TraderDbContext, TraderSymbol, Guid> _symbolRepository;
        private readonly IRepository<TraderDbContext, TraderAccount, Guid> _accountRepository;
        private readonly IUnitOfWork<TraderDbContext> _uow;
        private readonly IEasyTraderClient _easyTrader;
        private readonly ISecretProtector _protector;
        private readonly ILogger<SymbolService> _logger;

        public SymbolService(
            IRepository<TraderDbContext, TraderSymbol, Guid> symbolRepository,
            IRepository<TraderDbContext, TraderAccount, Guid> accountRepository,
            IUnitOfWork<TraderDbContext> uow,
            IEasyTraderClient easyTrader,
            ISecretProtector protector,
            ILogger<SymbolService> logger)
        {
            _symbolRepository = symbolRepository;
            _accountRepository = accountRepository;
            _uow = uow;
            _easyTrader = easyTrader;
            _protector = protector;
            _logger = logger;
        }

        /* ═══════════════════ Commands ═══════════════════ */

        public async Task<Guid> CreateSymbolAsync(
            string symbolName, string symbolIsin,
            long price, long quantity,
            int side, int validityType,
            decimal commission, int orderModelType, int orderFrom)
        {
            var symbol = TraderSymbol.Create(
                symbolName, symbolIsin,
                price, quantity,
                (OrderSide)side, validityType,
                commission, orderModelType, orderFrom);

            await _symbolRepository.AddAsync(symbol);
            return symbol.Id;
        }

        public async Task<Guid> UpdateSymbolAsync(
            Guid id,
            string symbolName, string symbolIsin,
            long price, long quantity,
            int side, int validityType,
            decimal commission, int orderModelType, int orderFrom)
        {
            var symbol = await _symbolRepository.GetByIdAsync(id)
                ?? throw new Exception($"Symbol {id} not found");

            symbol.SetInfo(
                symbolName, symbolIsin,
                price, quantity,
                (OrderSide)side, validityType,
                commission, orderModelType, orderFrom);

            await _symbolRepository.UpdateAsync(symbol);
            return symbol.Id;
        }

        public async Task<bool> DeleteSymbolAsync(Guid id)
        {
            var symbol = await _symbolRepository.GetByIdAsync(id)
                ?? throw new Exception($"Symbol {id} not found");

            await _symbolRepository.DeleteAsync(symbol);
            return true;
        }

        public async Task SaveAsync() => await _uow.SaveChangesAsync();

        /* ═══════════════════ Queries ═══════════════════ */

        public async Task<IReadOnlyList<SymbolInfoView>> GetSymbolListAsync()
        {
            var symbols = await _symbolRepository.GetAllAsync();

            return symbols
                .OrderBy(s => s.SymbolName)
                .Select(s => new SymbolInfoView
                {
                    Id = s.Id,
                    SymbolName = s.SymbolName,
                    SymbolIsin = s.SymbolIsin,
                    Price = s.Price,
                    Quantity = s.Quantity,
                    Side = (int)s.Side,
                    ValidityType = s.ValidityType,
                    Commission = s.Commission,
                    OrderModelType = s.OrderModelType,
                    OrderFrom = s.OrderFrom,
                })
                .ToList();
        }

        public async Task<MarketSymbolInfoView> GetMarketInfoAsync(string symbolIsin)
        {
            // پیدا کردن اولین حساب با توکن معتبر
            var accounts = await _accountRepository.GetAllAsync();
            var account = accounts.FirstOrDefault(
                a => a.GetTokenStatus() == TokenStatus.Valid)
                ?? throw new Exception("No account with valid token available");

            var token = _protector.Unprotect(account.EncryptedToken!);

            _logger.LogDebug(
                "Fetching market info for {Isin} using account {AccountId}",
                symbolIsin, account.Id);

            var result = await _easyTrader.GetSymbolInfoAsync(token, symbolIsin);

            return new MarketSymbolInfoView
            {
                SymbolIsin = result.SymbolIsin,
                HighAllowedPrice = result.HighAllowedPrice,
                LowAllowedPrice = result.LowAllowedPrice,
                LastTradedPrice = result.LastTradedPrice,
                ClosingPrice = result.ClosingPrice,
                FirstTradedPrice = result.FirstTradedPrice,
                TradeDate = result.TradeDate,
                FetchedAt = result.FetchedAt,
            };
        }
    }
}