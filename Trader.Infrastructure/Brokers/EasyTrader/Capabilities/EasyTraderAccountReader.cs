using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Dtos.Account;
using Trader.Application.Dtos.Finance;
using Trader.Application.Dtos.MarketData;
using Trader.Application.Dtos.Order;
using Trader.ExternalApiClient;
using Trader.ExternalApiClient.EasyTrader;
using Trader.Infrastructure.Brokers.EasyTrader.Internal;

namespace Trader.Infrastructure.Brokers.EasyTrader.Capabilities
{
    internal class EasyTraderAccountReader
        : EasyTraderCapabilityBase, IBrokerAccountReader
    {
        public EasyTraderAccountReader(
            EasyTraderApiClient api,
            ILogger<EasyTraderAccountReader> logger)
            : base(api, logger) { }

        public async Task<MoneyDto> GetCashBalanceAsync(
            ExternalSystemSession session, CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetMoneyAsync(s.AccessToken, ct);

            return new MoneyDto
            {
                T0 = wire.T0,
                T1 = wire.T1,
                T2 = wire.T2,
                BuyPowerT0 = wire.BuyPowerT0,
                BuyPowerT1 = wire.BuyPowerT1,
                BuyPowerT2 = wire.BuyPowerT2,
                BlockT2 = wire.BlockT2,
                WithdrawBlockT2 = wire.WithdrawBlockT2,
                MarginBlock = wire.MarginBlock,
                Credit = wire.Credit,
                AvandCredit = wire.AvandCredit,
                WalletWithdrawBalanceT0 = wire.WalletWithdrawBalanceT0,
                WarrantValueCredit = wire.WarrantValueCredit,
                HamiBalance = wire.HamiBalance,
                Block = wire.Block,
            };
        }

        public async Task<List<PortfolioPerformanceDto>> GetPortfolioPerformanceAsync(
            ExternalSystemSession session, CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetPerformanceAsync(s.AccessToken, ct);

            if (wire.Items is null || wire.Items.Count == 0) return new();

            return wire.Items.Select(p => new PortfolioPerformanceDto
            {
                SymbolIsin = p.SymbolIsin ?? "",
                SymbolName = p.SymbolName ?? "",
                Date = p.Date ?? "",
                PersianDate = p.PersianDate,
                Asset = p.Asset,
                BonusShareVolume = p.BonusShareVolume,
                StockRightVolume = p.StockRightVolume,
                PeriodicDPSAmount = p.PeriodicDPSAmount,
                LastDPSAmount = p.LastDPSAmount,
                TotalDPSAmount = p.TotalDPSAmount,
                BreakevenPoint = p.BreakevenPoint,
                BreakevenWithoutDps = p.BreakevenWithoutDps,
                PeriodicBuyAveragePrice = p.PeriodicBuyAveragePrice,
                PeriodicBuyAveragePriceWithoutDps = p.PeriodicBuyAveragePriceWithoutDps,
                PeriodicBuyAmount = p.PeriodicBuyAmount,
                PeriodicSellAveragePrice = p.PeriodicSellAveragePrice,
                PeriodicSellAmount = p.PeriodicSellAmount,
                PeriodicBoughtVolume = p.PeriodicBoughtVolume,
                PeriodicSoldVolume = p.PeriodicSoldVolume,
                PeriodicRealizedProfitAndLoss = p.PeriodicRealizedProfitAndLoss,
                PeriodicRealizedProfitAndLossWithoutDps = p.PeriodicRealizedProfitAndLossWithoutDps,
                TotalBuyAveragePrice = p.TotalBuyAveragePrice,
                TotalBuyAveragePriceWithoutDps = p.TotalBuyAveragePriceWithoutDps,
                TotalBuyAmount = p.TotalBuyAmount,
                TotalSellAveragePrice = p.TotalSellAveragePrice,
                TotalSellAmount = p.TotalSellAmount,
                TotalBoughtVolume = p.TotalBoughtVolume,
                TotalSoldVolume = p.TotalSoldVolume,
                TotalRealizedProfitAndLoss = p.TotalRealizedProfitAndLoss,
                TotalRealizedProfitAndLossWithoutDps = p.TotalRealizedProfitAndLossWithoutDps,
                DailyBuyAveragePrice = p.DailyBuyAveragePrice,
                DailySellAveragePrice = p.DailySellAveragePrice,
                DailyBoughtVolume = p.DailyBoughtVolume,
                DailySoldVolume = p.DailySoldVolume,
                PeriodicBuyNetPrice = p.PeriodicBuyNetPrice,
                PeriodicSellNetPrice = p.PeriodicSellNetPrice,
                TotalBuyNetPrice = p.TotalBuyNetPrice,
                TotalSellNetPrice = p.TotalSellNetPrice,
            }).ToList();
        }

        private async Task<ClientAppSettingDto> GetClientAppSettingAsync(
            ExternalSystemSession session, CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetClientAppSettingAsync(s.AccessToken, ct);

            return new ClientAppSettingDto
            {
                LightTheme = wire.LightTheme,
                BuyQuantity = wire.BuyQuantity,
                SellQuantity = wire.SellQuantity,
                Tick = wire.Tick,
                TickType = wire.TickType ?? "",
                PriceFromHeadline = wire.PriceFromHeadline,
                OrderConfirmation = wire.OrderConfirmation,
                DivideOrderToMultiple = wire.DivideOrderToMultiple,
                NotchUp = wire.NotchUp,
                NotchDown = wire.NotchDown,
                PageSize = wire.PageSize,
                ApplyCommissionInPortfolio = wire.ApplyCommissionInPortfolio,
                UseClosingPriceInPortfolioTotalValue = wire.UseClosingPriceInPortfolioTotalValue,
                ShowNotifications = wire.ShowNotifications,
                DataTracker = wire.DataTracker,
                UsePersianNumber = wire.UsePersianNumber,
                NoSleep = wire.NoSleep,
                NoBalance = wire.NoBalance,
                UserStatusBarToUp = wire.UserStatusBarToUp,
                PortfolioBasedOnLastPositivePeriod = wire.PortfolioBasedOnLastPositivePeriod,
                PortfolioTotalValueCalculateType = wire.PortfolioTotalValueCalculateType,
                KeepOrderFormAfterSubmit = wire.KeepOrderFormAfterSubmit,
                PriceFromHeadlineSide = wire.PriceFromHeadlineSide,
                BlinkOnDataChange = wire.BlinkOnDataChange,
                PortfolioBuyAveragePriceCalculationType = wire.PortfolioBuyAveragePriceCalculationType,
                PortfolioCalculationType = wire.PortfolioCalculationType,
                SoldPortfolioBuyAveragePriceCalculationType = wire.SoldPortfolioBuyAveragePriceCalculationType,
                SoldPortfolioCalculationType = wire.SoldPortfolioCalculationType,
                Volatility = wire.Volatility,
                InterestRate = wire.InterestRate,
                ShowAsLastPeriodAsset = wire.ShowAsLastPeriodAsset,
            };
        }

        private Task<bool> IsCreditCustomerAsync(
            ExternalSystemSession session, CancellationToken ct = default)
        {
            var s = AsSession(session);
            return Api.IsCreditCustomerAsync(s.AccessToken, ct);
        }

        private async Task<PaymentAccountBalancesDto> GetPaymentAccountBalancesAsync(
            ExternalSystemSession session, CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await Api.GetPaymentAccountBalancesAsync(s.AccessToken, ct);

            return new PaymentAccountBalancesDto
            {
                TotalBalanceRial = wire.TotalBalance,
                IsCustomerConstraintRestricted = wire.IsCustomerConstraintRestricted,
                BankAccounts = (wire.BankAccounts ?? new()).Select(b => new BankAccountDto
                {
                    Id = b.Id,
                    AccountNumber = b.AccountNumber ?? "",
                    ShebaNumber = b.ShebaNumber ?? "",
                    CardNumber = b.CardNumber,
                    BankName = b.BankName ?? "",
                    BankCode = b.BankCode ?? "",
                    BankTitle = b.BankTitle ?? "",
                    IsActive = b.IsActive,
                    IsPending = b.IsPending,
                    ShebaInquiry = b.ShebaInquiry,
                    Status = b.Status ?? "",
                }).ToList(),
                BalancesPerDate = (wire.AccountBalancePerDate ?? new()).Select(d => new AccountBalancePerDateDto
                {
                    EffectiveDate = d.EffectiveDate,
                    PerformDate = DateOnly.TryParse(d.PerformDate, out var pd) ? pd : default,
                    AvailableBalanceRial = d.AvailableBalance,
                    MaxSingleRequestAmountRial = d.MaxSingleRequestAmount,
                    MaxTotalRequestAmountRial = d.MaxTotalRequestAmount,
                    MaxTotalRequestCount = d.MaxTotalRequestCount,
                    HasImeWallet = d.HasImeWallet,
                    PerBank = (d.AccountBalancePerBank ?? new()).Select(pb => new AccountBalancePerBankDto
                    {
                        BankAccountId = pb.BankAccountId,
                        IsBankAvailable = pb.IsBankAvailable,
                        IsRequestConstraintRestricted = pb.IsRequestConstraintRestricted,
                        RequestRestrictionDetail = pb.RequestRestrictionDetail,
                        SingleRequestAmountRial = pb.SingleRequestAmount,
                        TotalRequestAmountRial = pb.TotalRequestAmount,
                        TotalRequestCount = pb.TotalRequestCount,
                    }).ToList(),
                }).ToList(),
            };
        }
    }
}