using Core.Application.Abstractions;
using Core.Application.Abstractions.Security;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services
{
    public class AccountService : IAccountCommandService, IAccountQueryService
    {
        private readonly IRepository<TraderDbContext, TraderAccount, Guid> _accountRepository;
        private readonly IUnitOfWork<TraderDbContext> _uow;
        private readonly IBrokerClientFactory _brokerFactory;
        private readonly ISecretProtector _protector;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IRepository<TraderDbContext, TraderAccount, Guid> accountRepository,
            IUnitOfWork<TraderDbContext> uow,
            IBrokerClientFactory brokerFactory,
            ISecretProtector protector,
            ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _uow = uow;
            _brokerFactory = brokerFactory;
            _protector = protector;
            _logger = logger;
        }

        /* ═══════════════════ Commands ═══════════════════ */

        public async Task<Guid> CreateAccountAsync(
            BrokerType broker,
            string name,
            string username,
            string password)
        {
            var encrypted = _protector.Protect(password);

            var account = TraderAccount.Create(
                broker,
                name,
                username,
                encrypted);

            await _accountRepository.AddAsync(account);

            _logger.LogInformation(
                "Created TraderAccount {Id} broker={Broker} name={Name}",
                account.Id, broker, name);

            return account.Id;
        }

        public async Task<Guid> UpdateAccountAsync(
            Guid id,
            string name,
            string username,
            string? password)
        {
            var account = await _accountRepository.GetByIdAsync(id)
                ?? throw new Exception($"Account {id} not found");

            var encryptedPassword = password is not null
                ? _protector.Protect(password)
                : null;

            account.SetInfo(name, username, encryptedPassword);

            await _accountRepository.UpdateAsync(account);

            _logger.LogInformation("Updated TraderAccount {Id}", id);

            return account.Id;
        }

        public async Task<bool> DeleteAccountAsync(Guid id)
        {
            var account = await _accountRepository.GetByIdAsync(id)
                ?? throw new Exception($"Account {id} not found");

            await _accountRepository.DeleteAsync(account);

            _logger.LogInformation("Deleted TraderAccount {Id}", id);

            return true;
        }

        public async Task LoginAsync(Guid id)
        {
            var account = await _accountRepository.GetByIdAsync(id)
                ?? throw new Exception($"Account {id} not found");

            var brokerClient = _brokerFactory.GetClient(account.Broker);
            var password = _protector.Unprotect(account.EncryptedPassword);

            _logger.LogInformation(
                "Logging in TraderAccount {Id} ({Username}) broker={Broker}",
                id, account.Username, account.Broker);

            /* ─── Login via broker (تمام مراحل OIDC/activation داخلش) ─── */
            var session = await brokerClient.LoginAsync(account.Username, password);

            /* ─── Serialize + Encrypt session ─── */
            var sessionJson = brokerClient.SerializeSession(session);
            var encryptedSession = _protector.Protect(sessionJson);

            account.SetSession(encryptedSession, session.ExpiresAt);

            await _accountRepository.UpdateAsync(account);

            _logger.LogInformation(
                "TraderAccount {Id} session updated, expiresAt={Exp}",
                id, session.ExpiresAt);
        }

        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }

        /* ═══════════════════ Queries ═══════════════════ */

        public async Task<IReadOnlyList<AccountInfoView>> GetAccountListAsync()
        {
            var accounts = await _accountRepository.GetAllAsync();

            return accounts
                .OrderBy(a => a.Name)
                .Select(a => new AccountInfoView
                {
                    Id = a.Id,
                    Broker = (int)a.Broker,
                    Name = a.Name,
                    Username = a.Username,
                    SessionStatus = a.GetSessionStatus()
                        .ToString()
                        .ToLowerInvariant(),
                    SessionExp = a.SessionExp?.ToUnixTimeMilliseconds(),
                })
                .ToList();
        }
    }
}