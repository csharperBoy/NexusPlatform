using Core.Application.Abstractions;
using Core.Application.Abstractions.Security;
using Core.Infrastructure.Repositories;      // IRepository, IUnitOfWork — namespace رو تطبیق بده
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;
using Trader.Application.EasyTrader.Exceptions;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services
{
    public class AccountService : IAccountCommandService, IAccountQueryService
    {
        private readonly IRepository<TraderDbContext, TraderAccount, Guid> _accountRepository;
        private readonly IUnitOfWork<TraderDbContext> _uow;
        private readonly IEasyTraderClient _easyTrader;
        private readonly ISecretProtector _protector;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IRepository<TraderDbContext, TraderAccount, Guid> accountRepository,
            IUnitOfWork<TraderDbContext> uow,
            IEasyTraderClient easyTrader,
            ISecretProtector protector,
            ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _uow = uow;
            _easyTrader = easyTrader;
            _protector = protector;
            _logger = logger;
        }

        /* ═══════════════════ Commands ═══════════════════ */

        public async Task<Guid> CreateAccountAsync(
            string name, string username, string password)
        {
            var encrypted = _protector.Protect(password);
            var account = TraderAccount.Create(name, username, encrypted);
            await _accountRepository.AddAsync(account);
            return account.Id;
        }

        public async Task<Guid> UpdateAccountAsync(
            Guid id, string name, string username, string? password)
        {
            var account = await _accountRepository.GetByIdAsync(id)
                ?? throw new Exception($"Account {id} not found");

            var encrypted = password is not null
                ? _protector.Protect(password)
                : null;

            account.SetInfo(name, username, encrypted);
            await _accountRepository.UpdateAsync(account);
            return account.Id;
        }

        public async Task<bool> DeleteAccountAsync(Guid id)
        {
            var account = await _accountRepository.GetByIdAsync(id)
                ?? throw new Exception($"Account {id} not found");

            await _accountRepository.DeleteAsync(account);
            return true;
        }

        public async Task LoginAsync(Guid id)
        {
            var account = await _accountRepository.GetByIdAsync(id)
                ?? throw new Exception($"Account {id} not found");

            var password = _protector.Unprotect(account.EncryptedPassword);

            _logger.LogInformation(
                "Logging in TraderAccount {Id} ({Username})",
                id, account.Username);

            var loginResult = await _easyTrader.LoginAsync(account.Username, password);

            var exp = DateTimeOffset.UtcNow.AddSeconds(loginResult.ExpiresIn);
            var encryptedToken = _protector.Protect(loginResult.AccessToken);

            account.SetToken(encryptedToken, exp);
            await _accountRepository.UpdateAsync(account);
        }

        public async Task ActivateAsync(Guid id)
        {
            var account = await _accountRepository.GetByIdAsync(id)
                ?? throw new Exception($"Account {id} not found");

            if (string.IsNullOrEmpty(account.EncryptedToken))
                throw new Exception("Account has no token");

            var token = _protector.Unprotect(account.EncryptedToken);

            _logger.LogInformation(
                "Activating token for TraderAccount {Id}", id);

            await _easyTrader.ActivateTokenAsync(token);
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
                    Name = a.Name,
                    Username = a.Username,
                    TokenStatus = a.GetTokenStatus().ToString().ToLowerInvariant(),
                    TokenExp = a.TokenExp?.ToUnixTimeMilliseconds(),
                })
                .ToList();
        }
    }
}