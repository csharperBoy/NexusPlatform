using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Brokers;
using Trader.Application.Dtos;
using Trader.Application.Dtos.Account;
using Trader.Application.Dtos.Finance;
using Trader.Application.Dtos.MarketData;
using Trader.Application.Dtos.Order;
using Trader.Domain.Enums;
using Trader.ExternalApiClient;
using Trader.ExternalApiClient.EasyTrader;

namespace Trader.Infrastructure.Brokers.EasyTrader
{
    public class EasyTraderBrokerClient : IBrokerClient
    {
        private readonly EasyTraderApiClient _api;
        private readonly ILogger<EasyTraderBrokerClient> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public EasyTraderBrokerClient(
            EasyTraderApiClient api,
            ILogger<EasyTraderBrokerClient> logger)
        {
            _api = api;
            _logger = logger;
        }

        public BrokerType BrokerType => BrokerType.EasyTrader;

        public HttpRequestMessage BuildOrderRequest(
    ExternalSystemSession session,
    string symbolIsin,
    long price,
    long quantity,
    int side)
        {
            var s = AsSession(session);
            return _api.BuildOrderRequest(s.AccessToken, symbolIsin, price, quantity, side);
        }

        public async Task<BrokerOrderResultDto> SendOrderWithRequestAsync(
            ExternalSystemSession session,
            HttpRequestMessage request,
            string symbolIsin,
            CancellationToken ct = default)
        {
            // session برای یکنواختی امضا نگه داشته شده؛ auth داخل خود request هست
            _ = AsSession(session);

            var result = await _api.SendOrderAsync(request, symbolIsin, ct);
            var err = result.Response.OmsError?.FirstOrDefault();

            return new BrokerOrderResultDto
            {
                IsSuccessful = result.Response.IsSuccessful,
                OrderId = result.Response.Id,
                Message = result.Response.Message,
                ErrorCode = err?.Code,
                ErrorName = err?.Name,
                FireAtUnixMs = result.FireAtUnixMs,
                ReceivedAtUnixMs = result.ReceivedAtUnixMs,
            };
        }
        /* ══════════════════════════════════════════════════
           SESSION
           ══════════════════════════════════════════════════ */
        public string SerializeSession(ExternalSystemSession session)
        {
            if (session is not EasyTraderSession et)
                throw new ArgumentException(
                    $"Expected {nameof(EasyTraderSession)}", nameof(session));
            return JsonSerializer.Serialize(et, JsonOpts);
        }

        public ExternalSystemSession DeserializeSession(string json)
        {
            var session = JsonSerializer.Deserialize<EasyTraderSession>(json, JsonOpts)
                ?? throw new InvalidOperationException("Session deserialization returned null");
            return session;
        }

        private static EasyTraderSession AsSession(ExternalSystemSession s)
            => s as EasyTraderSession
               ?? throw new ArgumentException("Session is not EasyTraderSession");

        /* ══════════════════════════════════════════════════
           LOGIN — ORCHESTRATION ONLY
           تصمیم می‌گیره کدوم API، با چه ترتیبی، با چه ورودی‌ای
           ══════════════════════════════════════════════════ */
        public async Task<ExternalSystemSession> LoginAsync(
            string username,
            string password,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                throw new EasyTraderException("Username/Password cannot be empty");

            // ─── PKCE setup ───
            var verifier = EasyTraderPkce.GenerateVerifier();
            var challenge = EasyTraderPkce.GenerateChallenge(verifier);
            var state = EasyTraderPkce.GenerateState();

            // ─── یک HTTP session مشترک برای کل flow ───
            using var loginSession = _api.CreateLoginSession();

            try
            {
                /* ─── ۱. GET /connect/authorize ─── */
                var authorizeRes = await _api.AuthorizeAsync(loginSession, challenge, state, ct);
                if (authorizeRes.StatusCode is not (302 or 303))
                    throw new EasyTraderException(
                        $"Authorize expected 302/303, got {authorizeRes.StatusCode}",
                        authorizeRes.StatusCode, authorizeRes.Body);

                var loginPath = authorizeRes.Location
                    ?? throw new EasyTraderException("Authorize response has no Location");

                /* ─── ۲. GET /Login ─── */
                var loginPage = await _api.GetLoginPageAsync(loginSession, loginPath, ct);
                var antiForgeryToken = EasyTraderApiClient.ExtractAntiForgeryToken(loginPage.Html)
                    ?? throw new EasyTraderException("__RequestVerificationToken not found");

                /* ─── ۳. POST /Login ─── */
                var postLoginRes = await _api.PostLoginAsync(
                    loginSession,
                    loginPage.LoginUrl,
                    username,
                    password,
                    antiForgeryToken,
                    ct);

                if (postLoginRes.StatusCode is not (302 or 303))
                    throw new EasyTraderException(
                        $"Login expected 302/303, got {postLoginRes.StatusCode}. " +
                        $"Body: {Truncate(postLoginRes.Body, 200)}",
                        postLoginRes.StatusCode, postLoginRes.Body);

                var callbackPath = postLoginRes.Location
                    ?? throw new EasyTraderException("Login response has no Location");

                /* ─── ۴. GET /connect/authorize/callback ─── */
                var callbackRes = await _api.CallbackAsync(loginSession, callbackPath, ct);

                if (callbackRes.StatusCode is not (302 or 303))
                    throw new EasyTraderException(
                        $"Callback expected 302/303, got {callbackRes.StatusCode}",
                        callbackRes.StatusCode);

                if (string.IsNullOrEmpty(callbackRes.Code))
                    throw new EasyTraderException("No 'code' in callback URL");

                if (callbackRes.State != state)
                    throw new EasyTraderException("State mismatch — possible CSRF");

                /* ─── ۵. POST /connect/token ─── */
                var tokenResponse = await _api.ExchangeTokenAsync(
                    loginSession, callbackRes.Code, verifier, ct);

                if (string.IsNullOrEmpty(tokenResponse.AccessToken))
                    throw new EasyTraderException("access_token missing in response");

                _logger.LogInformation(
                    "EasyTrader login successful, expiresIn={Expires}s",
                    tokenResponse.ExpiresIn);

                /* ─── ۶. POST /same-login (activation) ─── */
                await _api.SameLoginAsync(tokenResponse.AccessToken, ct);

                var exp = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                return new EasyTraderSession(tokenResponse.AccessToken, exp);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "EasyTrader login failed: network/SSL error");
                throw;
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "EasyTrader login timed out");
                throw new EasyTraderException("EasyTrader login timed out", ex);
            }
        }

        /* ══════════════════════════════════════════════════
           LATENCY
           ══════════════════════════════════════════════════ */
        public async Task<BrokerTimeMeasurement> MeasureLatencyAsync(
            ExternalSystemSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var clientTs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var response = await _api.GetServerTimeAsync(s.AccessToken, clientTs, ct);
            sw.Stop();

            var rtt = sw.ElapsedMilliseconds;

            return new BrokerTimeMeasurement(
                Diff: response.Diff,
                OneWayLatencyMs: rtt / 2,
                RttMs: rtt);
        }

        /* ══════════════════════════════════════════════════
           SYMBOL INFO
           ══════════════════════════════════════════════════ */
        public async Task<SymbolMarketDataDto> GetSymbolInfoAsync(
            ExternalSystemSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetSymbolInfoAsync(s.AccessToken, symbolIsin, ct);

            return new SymbolMarketDataDto
            {
                SymbolName = symbolIsin,
                HighAllowedPrice = wire.HighAllowedPrice,
                LowAllowedPrice = wire.LowAllowedPrice,
                LastTradedPrice = wire.LastTradedPrice,
                ClosingPrice = wire.ClosingPrice,
                FirstTradedPrice = wire.FirstTradedPrice,
                TradeDate = wire.TradeDate,
                FetchedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };
        }

        /* ══════════════════════════════════════════════════
           ORDER
           ══════════════════════════════════════════════════ */
        public Task<BrokerOrderResultDto> SendBuyOrderAsync(
            ExternalSystemSession session, string symbolIsin, long price, long quantity,
            CancellationToken ct = default)
            => SendOrderLegacyAsync(session, symbolIsin, price, quantity, side: 0, ct);

        public Task<BrokerOrderResultDto> SendSellOrderAsync(
            ExternalSystemSession session, string symbolIsin, long price, long quantity,
            CancellationToken ct = default)
            => SendOrderLegacyAsync(session, symbolIsin, price, quantity, side: 1, ct);

        private async Task<BrokerOrderResultDto> SendOrderLegacyAsync(
             ExternalSystemSession session, string symbolIsin, long price, long quantity,
             int side, CancellationToken ct)
        {
            var request = BuildOrderRequest(session, symbolIsin, price, quantity, side);
            return await SendOrderWithRequestAsync(session, request, symbolIsin, ct);
        }

        

        /* ══════════════════════════════════════════════════
           CANDLES
           ══════════════════════════════════════════════════ */
        public async Task<List<CandleDto>> GetCandlesAsync(
            ExternalSystemSession session,
            string symbolIsin,
            int days = 1,
            int intervalMinutes = 1,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetChartHistoryAsync(
                s.AccessToken, symbolIsin, days, intervalMinutes, ct);

            return wire.Data.Select(d => new CandleDto
            {
                T = d.T,
                O = d.O,
                H = d.H,
                L = d.L,
                C = d.C,
                V = d.V,
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           RETURN CHART
           ══════════════════════════════════════════════════ */
        public async Task<ReturnChartDto> GetReturnChartAsync(
            ExternalSystemSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetReturnChartDataAsync(s.AccessToken, symbolIsin, ct);

            return new ReturnChartDto
            {
                LastTradedPrice = wire.LastTradedPrice,
                E30 = wire.E30,
                E90 = wire.E90,
                E360 = wire.E360,
                MaturityDay = wire.MaturityDay,
                DaysToMaturity = wire.DaysToMaturity,
                ReturnToMaturity = wire.ReturnToMaturity,
            };
        }

        /* ══════════════════════════════════════════════════
           TECHNICAL ANALYSIS
           ══════════════════════════════════════════════════ */
        public async Task<TechnicalAnalysisDto> GetTechnicalAnalysisAsync(
            ExternalSystemSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetTechnicalAnalysisAsync(s.AccessToken, symbolIsin, ct);

            return new TechnicalAnalysisDto
            {
                TotalScore = new TechnicalScoreDto
                {
                    Cat = wire.TotalScore?.Cat ?? "totalscore",
                    Value = wire.TotalScore?.Value ?? 0,
                    State = wire.TotalScore?.State ?? "Neutral",
                },
                CategoryScore = (wire.CategoryScore ?? new()).Select(c => new TechnicalCategoryScoreDto
                {
                    Cat = c.Cat ?? "",
                    CatFa = c.CatFa ?? "",
                    State = c.State ?? "Neutral",
                }).ToList(),
            };
        }

        /* ══════════════════════════════════════════════════
           IND/INST TRADE
           ══════════════════════════════════════════════════ */
        public async Task<IndInstTradeDto> GetIndInstTradeAsync(
            ExternalSystemSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetIndInstTradeAsync(s.AccessToken, symbolIsin, ct);

            return new IndInstTradeDto
            {
                SymbolIsin = wire.SymbolISIN,
                IndBuyVolume = ParseLong(wire.IndBuyVolume),
                IndBuyNumber = ParseLong(wire.IndBuyNumber),
                IndSellVolume = ParseLong(wire.IndSellVolume),
                IndSellNumber = ParseLong(wire.IndSellNumber),
                InsBuyVolume = ParseLong(wire.InsBuyVolume),
                InsBuyNumber = ParseLong(wire.InsBuyNumber),
                InsSellVolume = ParseLong(wire.InsSellVolume),
                InsSellNumber = ParseLong(wire.InsSellNumber),
                Date = wire.Date,
            };
        }

        /* ══════════════════════════════════════════════════
           IND/INST ANALYSIS
           ══════════════════════════════════════════════════ */
        public async Task<IndInstAnalysisDto> GetIndInstAnalysisAsync(
            ExternalSystemSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetIndInstAnalysisAsync(s.AccessToken, symbolIsin, ct);

            if (wire is null)
                throw new EasyTraderException(
                    $"IndInst analysis returned empty for isin={symbolIsin}");

            return new IndInstAnalysisDto
            {
                IndBuyVol = wire.IndBuyVol,
                IndBuyPow = wire.IndBuyPow,
                IndSellVol = wire.IndSellVol,
                InsSellVol = wire.InsSellVol,
                InsBuyVol = wire.InsBuyVol,
                BidPres = wire.BidPres,
                NetInd = wire.NetInd,
                BuyPerInd = wire.BuyPerInd,
                SellPerInd = wire.SellPerInd,
                DiffValInd = wire.DiffValInd,
            };
        }

        /* ══════════════════════════════════════════════════
           IND TRADING TREND
           ══════════════════════════════════════════════════ */
        public async Task<List<IndTradingTrendDto>> GetIndTradingTrendAsync(
            ExternalSystemSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var list = await _api.GetIndTradingTrendAsync(s.AccessToken, symbolIsin, ct);

            return list.Select(x => new IndTradingTrendDto
            {
                Type = x.Type ?? "",
                Date = x.Date ?? "",
                Val = x.Val,
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           MARKET SHEET SUM
           ══════════════════════════════════════════════════ */
        public async Task<MarketSheetSumDto> GetMarketSheetSumAsync(
            ExternalSystemSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetMarketSheetSumAsync(s.AccessToken, symbolIsin, ct);

            return new MarketSheetSumDto
            {
                BuyVolume = wire.BuyVolume,
                BuyCount = wire.BuyCount,
                SellVolume = wire.SellVolume,
                SellCount = wire.SellCount,
            };
        }

        /* ══════════════════════════════════════════════════
           CASH BALANCE
           ══════════════════════════════════════════════════ */
        public async Task<MoneyDto> GetCashBalanceAsync(
            ExternalSystemSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetMoneyAsync(s.AccessToken, ct);

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

        /* ══════════════════════════════════════════════════
           PORTFOLIO PERFORMANCE
           ══════════════════════════════════════════════════ */
        public async Task<List<PortfolioPerformanceDto>> GetPortfolioPerformanceAsync(
            ExternalSystemSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetPerformanceAsync(s.AccessToken, ct);

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

        /* ══════════════════════════════════════════════════
           TRADING TIMES
           ══════════════════════════════════════════════════ */
        public async Task<List<TradingTimeDto>> GetTradingTimesAsync(
            ExternalSystemSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetTradingTimeAsync(s.AccessToken, ct);

            if (wire.Items is null) return new();

            return wire.Items.Select(t => new TradingTimeDto
            {
                Title = t.Title ?? new(),
                Days = t.Days ?? new(),
                TradingTimeType = t.TradingTimeType,
                StartPreTradingTime = t.StartPreTradingTime,
                EndPreTradingTime = t.EndPreTradingTime,
                StartTradingTime = t.StartTradingTime,
                EndTradingTime = t.EndTradingTime,
                StartTalTime = t.StartTalTime,
                EndTalTime = t.EndTalTime,
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           CLIENT APP SETTING
           ══════════════════════════════════════════════════ */
        public async Task<ClientAppSettingDto> GetClientAppSettingAsync(
            ExternalSystemSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetClientAppSettingAsync(s.AccessToken, ct);

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

        /* ══════════════════════════════════════════════════
           IS CREDIT CUSTOMER
           ══════════════════════════════════════════════════ */
        public Task<bool> IsCreditCustomerAsync(
            ExternalSystemSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            return _api.IsCreditCustomerAsync(s.AccessToken, ct);
        }

        /* ══════════════════════════════════════════════════
           INDUSTRY POSITIVE / NEGATIVE
           ══════════════════════════════════════════════════ */
        public async Task<List<IndustryPositiveNegativeDto>> GetIndustryPositiveNegativeAsync(
            ExternalSystemSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetPositiveNegativeSymbolsAsync(s.AccessToken, ct);

            return wire.Select(x => new IndustryPositiveNegativeDto
            {
                IndustryCode = x.IndustryCode ?? "",
                IndustryName = x.IndustryName ?? "",
                NegativeCount = x.NegativeCount,
                ZeroCount = x.ZeroCount,
                PositiveCount = x.PositiveCount,
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           TSE INDEX
           ══════════════════════════════════════════════════ */
        public async Task<List<TseIndexDto>> GetTseIndexAsync(
            ExternalSystemSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetTseIndexAsync(s.AccessToken, ct);

            return wire.Select(x => new TseIndexDto
            {
                SymbolIsin = x.SymbolIsin ?? "",
                SymbolTitle = x.SymbolTitle ?? "",
                DayOfEvent = DateTimeOffset.TryParse(x.DayOfEvent, out var dt) ? dt : default,
                IndexChanges = x.IndexChanges,
                LastIndexValue = x.LastIndexValue,
                PercentVariation = x.PercentVariation,
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           MARKET WATCH
           ══════════════════════════════════════════════════ */
        public async Task<List<MarketWatchCategoryDto>> GetMarketWatchAsync(
            ExternalSystemSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetMarketWatchAsync(s.AccessToken, ct);

            return wire.Select(x => new MarketWatchCategoryDto
            {
                Id = (x.Id ?? "").Trim(),
                Name = x.Name ?? "",
                Order = x.Order,
                IsDefault = x.IsDefault,
                CustomerIsin = x.CustomerIsin,
                CreateDateTime = DateTimeOffset.TryParse(x.CreateDateTime, out var dt) ? dt : null,
                SymbolIsins = (x.WatchCategorySymbols ?? new())
                    .Where(w => !string.IsNullOrEmpty(w.SymbolIsin))
                    .Select(w => w.SymbolIsin!)
                    .ToList(),
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           BATCH MARKET DATA
           ══════════════════════════════════════════════════ */
        public async Task<List<BatchMarketDataItemDto>> GetBatchMarketDataAsync(
            ExternalSystemSession session,
            IReadOnlyList<string> symbolIsins,
            CancellationToken ct = default)
        {
            if (symbolIsins is null || symbolIsins.Count == 0)
                return new();

            var s = AsSession(session);
            var wire = await _api.GetMarketDataGraphQLAsync(s.AccessToken, symbolIsins, ct);

            return wire.Select(x => new BatchMarketDataItemDto
            {
                SymbolIsin = x.SymbolIsin ?? "",
                StateCode = x.StateCode ?? "",
                LastTradedPrice = x.LastTradedPrice,
                ClosingPrice = x.ClosingPrice,
                FeeOfPreviousDaysClosingPrice = x.FeeOfPreviousDaysClosingPrice,
                PriceVar = x.PriceVar,
                BuyRatio = ParseDecimal(x.BuyRatio),
                SellRatio = ParseDecimal(x.SellRatio),
                HighPrice = x.HighPrice,
                LowPrice = x.LowPrice,
                FirstTradedPrice = x.FirstTradedPrice,
                LowAllowedPrice = x.LowAllowedPrice,
                HighAllowedPrice = x.HighAllowedPrice,
                BestBuyPrice = x.BestBuyPrice,
                BestBuyQuantity = x.BestBuyQuantity,
                BestSellPrice = x.BestSellPrice,
                BestSellQuantity = x.BestSellQuantity,
                TotalTradeValue = x.TotalTradeValue,
                TotalNumberOfTrades = x.TotalNumberOfTrades,
                TotalNumberOfSharesTraded = x.TotalNumberOfSharesTraded,
                MinValidBuyVolume = x.MinValidBuyVolume,
                MaxValidBuyVolume = x.MaxValidBuyVolume,
                MinValidSellVolume = x.MinValidSellVolume,
                MaxValidSellVolume = x.MaxValidSellVolume,
                PriceTickSize = x.PriceTickSize,
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           ORDER HISTORY
           ══════════════════════════════════════════════════ */
        public async Task<PagedResult<OrderHistoryItemDto>> GetOrderHistoryAsync(
            ExternalSystemSession session,
            OrderHistoryQuery query,
            CancellationToken ct = default)
        {
            var s = AsSession(session);

            var sieve = new SieveRequest
            {
                Page = query.Page,
                PageSize = query.PageSize,
                Sort = new SieveSort { Column = "createDateTime", Dir = 1 },
            };

            if (query.OrderStateRawFilter is { Count: > 0 })
            {
                sieve.Filters.Add(new SieveFilter
                {
                    Column = "orderState",
                    Operator = 9,
                    Value = query.OrderStateRawFilter,
                });
            }

            if (query.SymbolIsins is { Count: > 0 })
            {
                sieve.Filters.Add(new SieveFilter
                {
                    Column = "symbolIsin",
                    Operator = 9,
                    Value = query.SymbolIsins,
                });
            }

            var payload = new OrderReportRequest
            {
                SieveModel = sieve,
                SearchTadbir = true,
            };

            var wire = await _api.GetOrderReportAsync(s.AccessToken, payload, ct);

            var items = (wire.Records ?? new())
                .Where(r => r.OrderReport is not null)
                .Select(r => MapOrderHistory(r))
                .ToList();

            return new PagedResult<OrderHistoryItemDto>
            {
                Items = items,
                TotalRecords = wire.TotalRecords,
                Page = query.Page,
                PageSize = query.PageSize,
            };
        }

        /* ══════════════════════════════════════════════════
           ORDER TRADES
           ══════════════════════════════════════════════════ */
        public async Task<List<OrderTradeDto>> GetOrderTradesAsync(
            ExternalSystemSession session,
            string orderId,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetOrderTradesAsync(s.AccessToken, orderId, ct);

            return wire.Select(x => new OrderTradeDto
            {
                OrderId = x.Isr ?? "",
                TradeNumber = x.TradeNumber,
                InternalId = x.Id ?? "",
                RequestId = x.RequestId,
                CustomerIsin = x.CustomerIsin ?? "",
                SymbolIsin = x.Isin ?? "",
                Side = x.Side,
                Price = x.Price,
                Quantity = x.Quantity,
                Remain = x.Remain,
                HasRemain = x.HasRemain,
                TradeDate = TryParseDate(x.Date),
                CreateDateTime = TryParseDate(x.CreateDateTime),
                CancelDateTime = TryParseDateOrNull(x.CancelDateTime),
                IsCanceled = x.IsCanceled,
                Hon = x.Hon,
                Partition = x.Partition,
                ConsumerIndex = x.ConsumerIndex,
                Origin = x.Origin,
            }).ToList();
        }

        /* ══════════════════════════════════════════════════
           PAYMENT ACCOUNT BALANCES
           ══════════════════════════════════════════════════ */
        public async Task<PaymentAccountBalancesDto> GetPaymentAccountBalancesAsync(
            ExternalSystemSession session,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetPaymentAccountBalancesAsync(s.AccessToken, ct);

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

        /* ══════════════════════════════════════════════════
           WITHDRAWAL REQUEST
           ══════════════════════════════════════════════════ */
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

            var raw = await _api.PostPaymentsAsync(s.AccessToken, payload, ct);

            return new WithdrawalResultDto
            {
                IsSuccessful = raw.IsSuccessful,
                StatusCode = raw.StatusCode,
                ErrorBody = string.IsNullOrWhiteSpace(raw.Body) ? null : Truncate(raw.Body, 400),
                FireAtUnixMs = raw.FireAtUnixMs,
                ReceivedAtUnixMs = raw.ReceivedAtUnixMs,
            };
        }

        /* ══════════════════════════════════════════════════
           CANCEL PAYMENT
           ══════════════════════════════════════════════════ */
        public async Task<PaymentCancelResultDto> CancelPaymentAsync(
            ExternalSystemSession session,
            long paymentId,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var raw = await _api.PostPaymentCancelAsync(s.AccessToken, paymentId, ct);

            return new PaymentCancelResultDto
            {
                PaymentId = paymentId,
                IsSuccessful = raw.IsSuccessful,
                StatusCode = raw.StatusCode,
                ErrorBody = string.IsNullOrWhiteSpace(raw.Body) ? null : Truncate(raw.Body, 400),
                FireAtUnixMs = raw.FireAtUnixMs,
                ReceivedAtUnixMs = raw.ReceivedAtUnixMs,
            };
        }

        /* ══════════════════════════════════════════════════
           FUNDAMENTAL ANALYSIS
           ══════════════════════════════════════════════════ */
        public async Task<FundamentalAnalysisDto> GetFundamentalAnalysisAsync(
            ExternalSystemSession session,
            string symbolIsin,
            CancellationToken ct = default)
        {
            var s = AsSession(session);
            var wire = await _api.GetFundamentalAnalysisAsync(s.AccessToken, symbolIsin, ct);

            return new FundamentalAnalysisDto
            {
                TotalScore = wire.Total?.Score ?? 0,
                Categories = (wire.Item ?? new()).Select(x => new FundamentalCategoryScoreDto
                {
                    Title = x.Title ?? "",
                    Score = x.Score,
                }).ToList(),
            };
        }

        /* ══════════════════════════════════════════════════
           HELPERS (فقط mapping / parse — بدون HTTP)
           ══════════════════════════════════════════════════ */
        private static long ParseLong(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            if (long.TryParse(value, out var l)) return l;
            if (double.TryParse(value,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var d))
            {
                return (long)Math.Round(d);
            }
            return 0;
        }

        private static decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0m;
            if (decimal.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var d))
                return d;
            return 0m;
        }

        private static OrderHistoryItemDto MapOrderHistory(OrderReportRecord r)
        {
            var o = r.OrderReport!;
            return new OrderHistoryItemDto
            {
                OrderId = o.OrderId ?? "",
                InternalId = o.Id ?? "",
                ReferenceId = o.ReferenceId ?? "",
                ParentId = o.ParentId ?? "",
                CorrelationId = o.CorrelationId ?? "",
                CustomerIsin = o.CustomerIsin ?? "",
                SymbolIsin = o.SymbolIsin ?? "",
                SymbolName = o.SymbolName ?? "",
                Price = o.Price,
                MeanPrice = o.MeanPrice,
                Quantity = o.Quantity,
                ExecutedQuantity = o.ExecutedQuantity,
                OrderValue = o.OrderValue,
                Side = o.Side,
                OrderStateRaw = o.OrderState,
                State = OrderStateKindExtensions.FromEasyTrader(o.OrderState),
                CreateDateTime = TryParseDate(o.CreateDateTime),
                ModifyDateTime = TryParseDateOrNull(o.ModifyDate),
                ValidityDate = TryParseDateOrNull(o.ValidityDate),
                Validity = o.Validity,
                IgnoreCheckingMoney = o.IgnoreCheckingMoney,
                TraderCredit = o.TraderCredit,
                Error = o.Error,
                Sources = (r.OrderSources ?? new()).Select(src => new OrderSourceDto
                {
                    ActionType = src.ActionType,
                    CreateDateTime = TryParseDate(src.CreateDateTime),
                    Ip = src.Ip ?? "",
                    OrderFrom = src.OrderFrom,
                }).ToList(),
            };
        }

        private static DateTimeOffset TryParseDate(string? s)
            => DateTimeOffset.TryParse(s, out var d) ? d : default;

        private static DateTimeOffset? TryParseDateOrNull(string? s)
            => DateTimeOffset.TryParse(s, out var d) ? d : null;

        private static string Truncate(string? s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s ?? string.Empty;
            return s.Length <= max ? s : s[..max] + "...";
        }
    }
}