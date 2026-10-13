using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Commands;
using Trader.Application.Dtos.Finance;
using Trader.ExternalApiClient;
using Trader.ExternalApiClient.EasyTrader;
using Trader.Infrastructure.Brokers.EasyTrader.Internal;

namespace Trader.Infrastructure.Brokers.EasyTrader.Capabilities
{
    internal class EasyTraderFinanceCommands
        : EasyTraderCapabilityBase, IBrokerFinanceCommands
    {
        public EasyTraderFinanceCommands(
            EasyTraderApiClient api,
            ILogger<EasyTraderFinanceCommands> logger)
            : base(api, logger) { }

        public async Task<WithdrawalResultDto> RequestWithdrawalAsync(
            ExternalSystemSession session,
            WithdrawalRequestDto request,
            CancellationToken ct = default)
        {
            var s = AsSession(session);

            var payload = new WithdrawalRequestPayload
            {
                BankAccountId = request.BankAccountId,
                Iban = request.Iban,
                Amount = request.AmountRial,
                PerformDate = request.PerformDate.ToString("yyyy-MM-dd"),
                IsImeRequest = request.IsImeRequest,
            };

            var raw = await Api.PostPaymentsAsync(s.AccessToken, payload, ct);

            return new WithdrawalResultDto
            {
                IsSuccessful = raw.IsSuccessful,
                StatusCode = raw.StatusCode,
                ErrorBody = string.IsNullOrWhiteSpace(raw.Body)
                    ? null : EasyTraderTextHelpers.Truncate(raw.Body, 400),
                FireAtUnixMs = raw.FireAtUnixMs,
                ReceivedAtUnixMs = raw.ReceivedAtUnixMs,
            };
        }

        public async Task<PaymentCancelResultDto> CancelPaymentAsync(
            ExternalSystemSession session,
            long paymentId,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var raw = await Api.PostPaymentCancelAsync(s.AccessToken, paymentId, ct);

            return new PaymentCancelResultDto
            {
                PaymentId = paymentId,
                IsSuccessful = raw.IsSuccessful,
                StatusCode = raw.StatusCode,
                ErrorBody = string.IsNullOrWhiteSpace(raw.Body)
                    ? null : EasyTraderTextHelpers.Truncate(raw.Body, 400),
                FireAtUnixMs = raw.FireAtUnixMs,
                ReceivedAtUnixMs = raw.ReceivedAtUnixMs,
            };
        }
    }
}