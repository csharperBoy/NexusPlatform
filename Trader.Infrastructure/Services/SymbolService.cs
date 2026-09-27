using Core.Application.Abstractions;
using Core.Application.Abstractions.Security;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Brokers;
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
        private readonly IBrokerClientFactory _brokerFactory;
        private readonly ISecretProtector _protector;
        private readonly ILogger<SymbolService> _logger;

        public SymbolService(
            IRepository<TraderDbContext, TraderSymbol, Guid> symbolRepository,
            IRepository<TraderDbContext, TraderAccount, Guid> accountRepository,
            IUnitOfWork<TraderDbContext> uow,
            IBrokerClientFactory brokerFactory,
            ISecretProtector protector,
            ILogger<SymbolService> logger)
        {
            _symbolRepository = symbolRepository;
            _accountRepository = accountRepository;
            _uow = uow;
            _brokerFactory = brokerFactory;
            _protector = protector;
            _logger = logger;
        }

        /* ═══════════════════ Commands ═══════════════════ */

        public async Task<Guid> CreateSymbolAsync(
            string symbolName,
            string symbolIsin,
            long price,
            long quantity,
            int side,
            int validityType,
            decimal commission,
            int orderModelType,
            int orderFrom)
        {
            var symbol = TraderSymbol.Create(
                symbolName,
                symbolIsin,
                price,
                quantity,
                (OrderSide)side,
                validityType,
                commission,
                orderModelType,
                orderFrom);

            await _symbolRepository.AddAsync(symbol);

            _logger.LogInformation(
                "Created TraderSymbol {Id} ({SymbolName})",
                symbol.Id, symbolName);

            return symbol.Id;
        }

        public async Task<Guid> UpdateSymbolAsync(
            Guid id,
            string symbolName,
            string symbolIsin,
            long price,
            long quantity,
            int side,
            int validityType,
            decimal commission,
            int orderModelType,
            int orderFrom)
        {
            var symbol = await _symbolRepository.GetByIdAsync(id)
                ?? throw new Exception($"Symbol {id} not found");

            symbol.SetInfo(
                symbolName,
                symbolIsin,
                price,
                quantity,
                (OrderSide)side,
                validityType,
                commission,
                orderModelType,
                orderFrom);

            await _symbolRepository.UpdateAsync(symbol);

            _logger.LogInformation("Updated TraderSymbol {Id}", id);

            return symbol.Id;
        }

        public async Task<bool> DeleteSymbolAsync(Guid id)
        {
            var symbol = await _symbolRepository.GetByIdAsync(id)
                ?? throw new Exception($"Symbol {id} not found");

            await _symbolRepository.DeleteAsync(symbol);

            _logger.LogInformation("Deleted TraderSymbol {Id}", id);

            return true;
        }

        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }

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

        public async Task<SymbolMarketDataDto> GetMarketInfoAsync(string symbolName)
        {
            var (brokerClient, session) = await GetDefaultSessionAsync();

            _logger.LogDebug(
                "Fetching market info for {Symbol} via {Broker}",
                symbolName, brokerClient.BrokerType);

            return await brokerClient.GetSymbolInfoAsync(session, symbolName);
        }

        /* ═══════════════════ Helpers ═══════════════════ */

        /// <summary>
        /// اولین حساب با session معتبر رو پیدا می‌کنه و session رو decrypt می‌کنه.
        /// </summary>
        private async Task<(IBrokerClient Client, BrokerSession Session)> GetDefaultSessionAsync()
        {
            var accounts = await _accountRepository.GetAllAsync();

            var account = accounts
                .Where(a => a.GetSessionStatus() == SessionStatus.Valid)
                .OrderBy(a => a.Name)
                .FirstOrDefault()
                ?? throw new Exception(
                    "No account with valid session available. Please login first.");

            var brokerClient = _brokerFactory.GetClient(account.Broker);
            var sessionJson = _protector.Unprotect(account.EncryptedSession!);
            var session = brokerClient.DeserializeSession(sessionJson);

            return (brokerClient, session);
        }
    }
}