این ها اینترفیس های عملیات هایی که میتونیم توی کارگزاری های مختلف انجام بدیم. من پیاده سازیشون هم کردم ولی دیگه کد اونهارو برات نمیفرستم ولی دارن کار میکنن الآن و همشون تست شدن:

using Trader.Application.Dtos.Finance;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Commands
{
        public interface IBrokerFinanceCommands : IBroker
        {
        /// <summary>
        /// درخواست برداشت وجه از حساب کاربری در کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<WithdrawalResultDto> RequestWithdrawalAsync(
                ExternalSystemSession session, WithdrawalRequestDto request,
                CancellationToken ct = default);

        /// <summary>
        /// درخواست لغو پرداخت از کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="paymentId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PaymentCancelResultDto> CancelPaymentAsync(
                ExternalSystemSession session, long paymentId,
                CancellationToken ct = default);
        }
    
}

using Trader.Application.Dtos;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Commands
{
        public interface IBrokerTradingCommands : IBroker
        {
        /// <summary>
        /// ارسال سفارش خرید به کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="price"></param>
        /// <param name="quantity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrokerOrderResultDto> SendBuyOrderAsync(
                ExternalSystemSession session, string symbolIsin, long price, long quantity,
                CancellationToken ct = default);

        /// <summary>
        /// ارسال سفارش فروش به کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="price"></param>
        /// <param name="quantity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrokerOrderResultDto> SendSellOrderAsync(
                ExternalSystemSession session, string symbolIsin, long price, long quantity,
                CancellationToken ct = default);

        /// <summary>
        /// ارسال سفارش خرید یا فروش به کارگزاری با استفاده از درخواست آماده شده
        /// </summary>
        /// <param name="session"></param>
        /// <param name="request"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrokerOrderResultDto> SendOrderWithRequestAsync(
           ExternalSystemSession session,
           HttpRequestMessage request,
           string symbolIsin,
           CancellationToken ct = default);

        /// <summary>
        ///  ساخت درخواست ارسال سفارش خرید یا فروش به کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="price"></param>
        /// <param name="quantity"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        HttpRequestMessage BuildOrderRequest(
           ExternalSystemSession session,
           string symbolIsin,
           long price,
           long quantity,
           int side);

        }
    
}

using Trader.Application.Dtos.Account;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Queries
{
        public interface IBrokerAccountReader : IBroker
        {
        /// <summary>
        /// دریافت قدرت خرید حساب کاربر
        /// </summary>
        /// <param name="session"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<MoneyDto> GetCashBalanceAsync(
                ExternalSystemSession session, CancellationToken ct = default);

        /// <summary>
        /// دریافت پرتفوی کاربر
        /// </summary>
        /// <param name="session"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<PortfolioPerformanceDto>> GetPortfolioPerformanceAsync(
                ExternalSystemSession session, CancellationToken ct = default);

        ///// <summary>
        ///// دریافت تنظیمات اپلیکیشن کاربر
        ///// </summary>
        ///// <param name="session"></param>
        ///// <param name="ct"></param>
        ///// <returns></returns>
        //Task<ClientAppSettingDto> GetClientAppSettingAsync(
        //        ExternalSystemSession session, CancellationToken ct = default);

        ///// <summary>
        ///// بررسی اینکه آیا کاربر مشتری اعتباری است یا خیر
        ///// </summary>
        ///// <param name="session"></param>
        ///// <param name="ct"></param>
        ///// <returns></returns>
        //Task<bool> IsCreditCustomerAsync(
        //        ExternalSystemSession session, CancellationToken ct = default);

        ///// <summary>
        ///// دریافت موجودی قابل برداشت برای هس حساب بانکی کاربر
        ///// </summary>
        ///// <param name="session"></param>
        ///// <param name="ct"></param>
        ///// <returns></returns>
        //Task<PaymentAccountBalancesDto> GetPaymentAccountBalancesAsync(
        //        ExternalSystemSession session, CancellationToken ct = default);
        }
    
}


using Trader.Application.Dtos.MarketData;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Queries
{
        public interface IBrokerAnalysisReader : IBroker
        {
        /// <summary>
        /// دریافت تحلیل تکنیکال یک نماد از کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TechnicalAnalysisDto> GetTechnicalAnalysisAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

        /// <summary>
        /// دریافت تحلیل بنیادی یک نماد از کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<FundamentalAnalysisDto> GetFundamentalAnalysisAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

        /// <summary>
        /// دریافت معاملات حقیقی و حقوقی یک نماد از کارگزاری
        ///  حجم/تعداد خرید و فروش حقیقی و حقوقی (لحظه‌ای)
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IndInstTradeDto> GetIndInstTradeAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

        /// <summary>
        /// دریافت تحلیل معاملات حقیقی و حقوقی یک نماد از کارگزاری  — نسبت‌ها و قدرت خریدار
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IndInstAnalysisDto> GetIndInstAnalysisAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

        /// <summary>
        /// دریافت روند معاملات حقیقی و حقوقی یک نماد از کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<IndTradingTrendDto>> GetIndTradingTrendAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);
        }
    
}
using Trader.Application.Dtos;
using Trader.Application.Dtos.MarketData;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Queries
{
        public interface IBrokerMarketDataReader : IBroker
        {
            Task<SymbolMarketDataDto> GetSymbolInfoAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

            Task<List<CandleDto>> GetCandlesAsync(
                ExternalSystemSession session, string symbolIsin,
                int days = 1, int intervalMinutes = 1, CancellationToken ct = default);

            Task<List<BatchMarketDataItemDto>> GetBatchMarketDataAsync(
                ExternalSystemSession session, IReadOnlyList<string> symbolIsins,
                CancellationToken ct = default);

            Task<List<TseIndexDto>> GetTseIndexAsync(
                ExternalSystemSession session, CancellationToken ct = default);

            Task<MarketSheetSumDto> GetMarketSheetSumAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

            Task<ReturnChartDto> GetReturnChartAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

            Task<List<TradingTimeDto>> GetTradingTimesAsync(
                ExternalSystemSession session, CancellationToken ct = default);
        }
    
}


using Trader.Application.Dtos;
using Trader.Application.Dtos.Order;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Queries
{
        public interface IBrokerOrderHistoryReader : IBroker
        {
            Task<PagedResult<OrderHistoryItemDto>> GetOrderHistoryAsync(
                ExternalSystemSession session, OrderHistoryQuery query,
                CancellationToken ct = default);

            Task<List<OrderTradeDto>> GetOrderTradesAsync(
                ExternalSystemSession session, string orderId,
                CancellationToken ct = default);
        }
    
}
using Trader.Application.Dtos.MarketData;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Queries
{
        public interface IBrokerWatchlistReader : IBroker
        {
            Task<List<MarketWatchCategoryDto>> GetMarketWatchAsync(
                ExternalSystemSession session, CancellationToken ct = default);

            Task<List<IndustryPositiveNegativeDto>> GetIndustryPositiveNegativeAsync(
                ExternalSystemSession session, CancellationToken ct = default);
        }
    
}

using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Brokers
{
        /// <summary>پایه‌ی همه‌ی اینترفیس‌های بروکر. فقط BrokerType رو می‌شناسه.</summary>
        public interface IBroker
        {
            BrokerType BrokerType { get; }
        }
    
}

using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Brokers
{
    /// <summary>
    /// Resolve یک capability بروکر بر اساس نوع بروکر در زمان اجرا.
    /// </summary>
    public interface IBrokerClientFactory
    {
        T Get<T>(BrokerType brokerType) where T : class, IBroker;
    }
}

using Trader.Application.Dtos;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers
{
        public interface IBrokerSessionManager : IBroker
        {
            Task<ExternalSystemSession> LoginAsync(
                string username,
                string password,
                CancellationToken ct = default);

            string SerializeSession(ExternalSystemSession session);

            ExternalSystemSession DeserializeSession(string json);

            /// <summary>
            /// زمان سرور رو می‌گیره + RTT. لازمه برای sync قبل از fire سفارش.
            /// </summary>
            Task<BrokerTimeMeasurement> MeasureLatencyAsync(
                ExternalSystemSession session,
                CancellationToken ct = default);
        }
    
}


dto هارو هم فعلا نمیفرستم برات ولی اونهارو هم اگر نیاز داری بدونی ساختارشون رو بگو تا کدهاشون رو برات بفرستم:


اینها هم اینترفیس های سرویس های یه سطح بالاترمون هستن که به نوعی از ترکیب امکاناتی که کارگزاری ها توی سرویس های بالا بهمون دادن و ریپازیتوری های دیتابیس عملیات های تکمیلی تر و منطق و فلو رو پیاده میکنیم توشون:
در اصل اینها سرویس هاییه که مستقیما توی کامند ها و کوئری هندلر ها صدا زده میشن.

using Trader.Domain.Enums;

namespace Trader.Application.Abstractions
{
    public interface IAccountCommandService
    {
        /// <summary>
        /// ساخت اکانت داخل دیتابیس
        /// </summary>
        /// <param name="broker"></param>
        /// <param name="name"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<Guid> CreateAccountAsync(
            BrokerType broker,
            string name,
            string username,
            string password);

        /// <summary>
        /// بروزرسانی اکانت داخل دیتابیس
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<Guid> UpdateAccountAsync(
            Guid id,
            string name,
            string username,
            string? password);

        /// <summary>
        /// حذف اکانت از دیتابیس
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteAccountAsync(Guid id);

        /// <summary>
        /// ورود به اکانت و دریافت توکن و ذخیره در دیتابیس
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task LoginAsync(Guid id);

        /// <summary>
        /// ذخیره تغییرات در دیتابیس ( داخل unit of work)
        /// </summary>
        /// <returns></returns>
        Task SaveAsync();
    }
}

using Trader.Application.Dtos;

namespace Trader.Application.Abstractions
{
    public interface IAccountQueryService
    {
        /// <summary>
        /// دریافت لیست حساب‌ها
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<AccountInfoView>> GetAccountListAsync();
    }
}

using Trader.Application.Dtos;

namespace Trader.Application.Abstractions
{
    public interface IExecutionLogQueryService
    {
        Task<ExecutionLogQueryResult> GetLogsAsync(
            Guid? planId,
            string? level,
            string? search,
            int skip,
            int take,
            CancellationToken ct = default);
    }
}


using Trader.Application.Dtos;

namespace Trader.Application.Abstractions
{
    public interface ISchedulePlanCommandService
    {
        /// <summary>
        /// ساخت برنامه زمانبندی جدید
        /// </summary>
        /// <param name="name"></param>
        /// <param name="date"></param>
        /// <param name="enabled"></param>
        /// <param name="autoLoginAt"></param>
        /// <param name="autoRefreshAt"></param>
        /// <param name="fireLeadMs"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        Task<Guid> CreateSchedulePlanAsync(
     string name,
     string date,
     bool enabled,
     string autoLoginAt,
     string autoRefreshAt,
     int fireLeadMs,
     List<ScheduledOrderItemDto> orders);

        /// <summary>
        /// بروزرسانی برنامه زمانبندی
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="date"></param>
        /// <param name="enabled"></param>
        /// <param name="autoLoginAt"></param>
        /// <param name="autoRefreshAt"></param>
        /// <param name="fireLeadMs"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        Task<Guid> UpdateSchedulePlanAsync(
            Guid id,
            string name,
            string date,
            bool enabled,
            string autoLoginAt,
            string autoRefreshAt,
            int fireLeadMs,
            List<ScheduledOrderItemDto> orders);

        /// <summary>
        /// حذف برنامه زمانبندی
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteSchedulePlanAsync(Guid id);

        /// <summary>
        /// فعالسازی برنامه زمانبندی و ایجاد متناظر آن در ماژول Scheduler جهت اجرا
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task EnableAsync(Guid id);
        /// <summary>
        /// غیرفعالسازی برنامه زمانبندی و حذف متناظر آن در ماژول Scheduler
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DisableAsync(Guid id);
        /// <summary>
        /// ذخیره تغییرات در دیتابیس
        /// </summary>
        /// <returns></returns>
        Task SaveAsync();
    }
}

البته ما اجرای برنامه های زمانبندی شده رو براش یک ماژول جدا داریم که این اینترفیس رو بهمون داده:

namespace Scheduler.Application.Abstractions
{
    public interface ISchedulerService
    {
        /// <summary>
        /// زمان‌بندی یک job جدید.
        /// </summary>
        /// <returns>شناسه‌ی job در سیستم scheduler</returns>
        Task<Guid> ScheduleAsync<TPayload>(
            TPayload payload,
            DateTimeOffset fireAt,
            string? queue = null,
            TimeSpan? preFireBuffer = null,
            CancellationToken ct = default)
            where TPayload : IScheduledJobPayload;

        /// <summary>لغو یک job زمان‌بندی‌شده (اگه هنوز اجرا نشده)</summary>
        Task<bool> CancelAsync(Guid jobId, CancellationToken ct = default);

        /// <summary>دریافت اطلاعات یک job</summary>
        Task<ScheduledJobInfo?> GetAsync(Guid jobId, CancellationToken ct = default);

        /// <summary>جستجوی jobها</summary>
        Task<IReadOnlyList<ScheduledJobInfo>> QueryAsync(
            SchedulerQuery query,
            CancellationToken ct = default);
    }
}
که ما داریم ازش استفاده میکنیم برای سرخطی زدن ها.

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

using Trader.Application.Dtos;

namespace Trader.Application.Abstractions
{
    public interface ISymbolQueryService
    {
        Task<IReadOnlyList<SymbolInfoView>> GetSymbolListAsync();

        Task<SymbolMarketDataDto> GetMarketInfoAsync(string symbolIsin);
    }
}

خب، این ها اینترفیس های اصلی و سرویس های سطح بالاتر ما هستن که با ترکیب امکانات کارگزاری ها و منطق خودمون، عملیات های مختلف رو انجام میدن. اگر نیاز داری که ساختار dto ها رو هم بدونی، میتونم برات اونها رو هم بفرستم. آیا میخوای که ساختار dto ها رو هم برات بفرستم؟

یک نمونه کامند و یک نمونه کوئری هم میفرستم که با ساختار و قالب کارم آشنا بشی وگرنه همشون به همین شکل هستن و دیگه کدهاشون رو نمیفرستم که طولانی پیامم:
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Domain.Enums;

namespace Trader.Application.Commands.Account
{
    public record CreateAccountCommand(
        int Broker,
        string Name,
        string Username,
        string Password
    ) : IRequest<Result<Guid>>;

    public class CreateAccountCommandHandler
        : IRequestHandler<CreateAccountCommand, Result<Guid>>
    {
        private readonly IAccountCommandService _accountService;
        private readonly ILogger<CreateAccountCommandHandler> _logger;

        public CreateAccountCommandHandler(
            IAccountCommandService accountService,
            ILogger<CreateAccountCommandHandler> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(
            CreateAccountCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var broker = (BrokerType)request.Broker;

                _logger.LogInformation(
                    "Creating TraderAccount: {Name} broker={Broker}",
                    request.Name, broker);

                var id = await _accountService.CreateAccountAsync(
                    broker,
                    request.Name,
                    request.Username,
                    request.Password);

                await _accountService.SaveAsync();

                _logger.LogInformation(
                    "TraderAccount created: {Id} ({Name})", id, request.Name);

                return Result<Guid>.Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create TraderAccount: {Name}", request.Name);
                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}

و این هم کوئری:
 
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;

namespace Trader.Application.Queries.Account
{
    public record GetAccountListQuery(string? Search = null)
        : IRequest<Result<IReadOnlyList<AccountInfoView>>>;

    public class GetAccountListQueryHandler
        : IRequestHandler<GetAccountListQuery, Result<IReadOnlyList<AccountInfoView>>>
    {
        private readonly IAccountQueryService _accountQueryService;
        private readonly ILogger<GetAccountListQueryHandler> _logger;

        public GetAccountListQueryHandler(
            IAccountQueryService accountQueryService,
            ILogger<GetAccountListQueryHandler> logger)
        {
            _accountQueryService = accountQueryService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<AccountInfoView>>> Handle(
            GetAccountListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting Trader Account List");

                var accounts = await _accountQueryService.GetAccountListAsync();
                return Result<IReadOnlyList<AccountInfoView>>.Ok(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Trader Account List");
                return Result<IReadOnlyList<AccountInfoView>>.Fail(ex.Message);
            }
        }
    }
}

اگر چیز بیشتری نیاز داری بگو بفرستم.

فعلا بنظرم نیاز نداری چون صرفا میخایم ببینیم داریم ساختار رو درست و اصولی میچینیم یا نه.

این هم ساختار کامل فایل های فعلی پروژه:
?   Trader.Application
?   ?   Trader.Application.csproj
?   ?   Abstractions
?   ?   ?   IAccountCommandService.cs
?   ?   ?   IAccountQueryService.cs
?   ?   ?   IExecutionLogQueryService.cs
?   ?   ?   ISchedulePlanCommandService.cs
?   ?   ?   ISchedulePlanQueryService.cs
?   ?   ?   IServerClockCommandService.cs
?   ?   ?   IServerClockQueryService.cs
?   ?   ?   ISymbolCommandService.cs
?   ?   ?   ISymbolQueryService.cs
?   ?   ?   Brokers
?   ?   ?   ?   IBroker.cs
?   ?   ?   ?   IBrokerClientFactory.cs
?   ?   ?   ?   IBrokerSessionManager.cs
?   ?   ?   ?   Commands
?   ?   ?   ?   ?   IBrokerFinanceCommands.cs
?   ?   ?   ?   ?   IBrokerTradingCommands.cs
?   ?   ?   ?   Queries
?   ?   ?   ?   ?   IBrokerAccountReader.cs
?   ?   ?   ?   ?   IBrokerAnalysisReader.cs
?   ?   ?   ?   ?   IBrokerMarketDataReader.cs
?   ?   ?   ?   ?   IBrokerOrderHistoryReader.cs
?   ?   ?   ?   ?   IBrokerWatchlistReader.cs
?   ?   Brokers
?   ?   ?   BrokerPaymentExtensions.cs
?   ?   ?   InMemoryWithdrawalGuard .cs
?   ?   ?   ISafeWithdrawalService.cs
?   ?   ?   IWithdrawalGuard.cs
?   ?   ?   WithdrawalValidator.cs
?   ?   Commands
?   ?   ?   Account
?   ?   ?   ?   CreateAccountCommand.cs
?   ?   ?   ?   DeleteAccountCommand.cs
?   ?   ?   ?   LoginAccountCommand.cs
?   ?   ?   ?   UpdateAccountCommand.cs
?   ?   ?   SchedulePlan
?   ?   ?   ?   CreateSchedulePlanCommand.cs
?   ?   ?   ?   DeleteSchedulePlanCommand.cs
?   ?   ?   ?   DisableSchedulePlanCommand.cs
?   ?   ?   ?   EnableSchedulePlanCommand.cs
?   ?   ?   ?   UpdateSchedulePlanCommand.cs
?   ?   ?   ServerClock
?   ?   ?   ?   SyncServerClockCommand.cs
?   ?   ?   Symbol
?   ?   ?   ?   CreateSymbolCommand.cs
?   ?   ?   ?   DeleteSymbolCommand.cs
?   ?   ?   ?   UpdateSymbolCommand.cs
?   ?   DependencyInjection
?   ?   ?   ServiceCollectionExtensions.cs
?   ?   Dtos
?   ?   ?   AccountInfoView.cs
?   ?   ?   BrokerOrderResultDto.cs
?   ?   ?   BrokerTimeMeasurement.cs
?   ?   ?   ExecutionLogInfoView.cs
?   ?   ?   PagedResult.cs
?   ?   ?   ScheduledOrderInfoView.cs
?   ?   ?   ScheduledOrderItemDto.cs
?   ?   ?   SchedulePlanInfoView.cs
?   ?   ?   ServerClockInfoView.cs
?   ?   ?   SymbolInfoView.cs
?   ?   ?   SymbolMarketDataDto.cs
?   ?   ?   Account
?   ?   ?   ?   ClientAppSettingDto.cs
?   ?   ?   ?   MoneyDto.cs
?   ?   ?   ?   PortfolioPerformanceDto.cs
?   ?   ?   Finance
?   ?   ?   ?   AccountBalancePerBankDto.cs
?   ?   ?   ?   AccountBalancePerDateDto.cs
?   ?   ?   ?   BankAccountDto.cs
?   ?   ?   ?   PaymentAccountBalancesDto.cs
?   ?   ?   ?   PaymentCancelResultDto.cs
?   ?   ?   ?   WithdrawalRequestDto.cs
?   ?   ?   ?   WithdrawalResultDto.cs
?   ?   ?   MarketData
?   ?   ?   ?   BatchMarketDataItemDto.cs
?   ?   ?   ?   CandleDto.cs
?   ?   ?   ?   FundamentalAnalysisDto.cs
?   ?   ?   ?   IndInstAnalysisDto.cs
?   ?   ?   ?   IndInstTradeDto.cs
?   ?   ?   ?   IndTradingTrendDto.cs
?   ?   ?   ?   IndustryPositiveNegativeDto.cs
?   ?   ?   ?   MarketSheetSumDto.cs
?   ?   ?   ?   MarketWatchCategoryDto.cs
?   ?   ?   ?   ReturnChartDto.cs
?   ?   ?   ?   TechnicalAnalysisDto.cs
?   ?   ?   ?   TradingTimeDto.cs
?   ?   ?   ?   TseIndexDto.cs
?   ?   ?   Order
?   ?   ?   ?   OrderHistoryItemDto.cs
?   ?   ?   ?   OrderHistoryQuery.cs
?   ?   ?   ?   OrderTradeDto.cs
?   ?   Queries
?   ?   ?   Account
?   ?   ?   ?   GetAccountListQuery.cs
?   ?   ?   ?   GetAccountsSelectionListQuery.cs
?   ?   ?   ExecutionLog
?   ?   ?   ?   GetExecutionLogsQuery.cs
?   ?   ?   SchedulePlan
?   ?   ?   ?   GetSchedulePlanByIdQuery.cs
?   ?   ?   ?   GetSchedulePlanListQuery.cs
?   ?   ?   ServerClock
?   ?   ?   ?   GetServerClockStatusQuery.cs
?   ?   ?   Symbol
?   ?   ?   ?   GetSymbolListQuery.cs
?   ?   ?   ?   GetSymbolMarketInfoQuery.cs
?   ?   ?   ?   GetSymbolsSelectionListQuery.cs
?   ?   Scheduler
?   ?   ?   PlanExecutionPayload.cs
?   ?   ?   Services
?   ?   ?   ?   IPlanExecutor.cs
?   Trader.Domain
?   ?   Trader.Domain.csproj
?   ?   Entities
?   ?   ?   ExecutionLog.cs
?   ?   ?   ScheduledOrder.cs
?   ?   ?   SchedulePlan.cs
?   ?   ?   TraderAccount.cs
?   ?   ?   TraderSymbol.cs
?   ?   Enums
?   ?   ?   BrokerType.cs
?   ?   ?   ExecutionLogLevel.cs
?   ?   ?   OrderMode.cs
?   ?   ?   OrderSide.cs
?   ?   ?   OrderStateKind.cs
?   ?   ?   SchedulePlanStatus.cs
?   ?   ?   SessionStatus.cs
?   Trader.ExternalApiClient
?   ?   ExternalSystemSession.cs
?   ?   Trader.ExternalApiClient.csproj
?   ?   DependencyInjection
?   ?   ?   ServiceCollectionExtensions.cs
?   ?   EasyTrader
?   ?   ?   EasyTraderApiClient.cs
?   ?   ?   EasyTraderApiHandles.cs
?   ?   ?   EasyTraderApiResults.cs
?   ?   ?   EasyTraderEndpoints.cs
?   ?   ?   EasyTraderException.cs
?   ?   ?   EasyTraderOptions.cs
?   ?   ?   EasyTraderOrderException.cs
?   ?   ?   EasyTraderPkce.cs
?   ?   ?   EasyTraderSession.cs
?   ?   ?   EasyTraderWireModels.cs
?   ?   ?   Enums
?   ?   ?   ?   EasyTraderOrderStateKind.cs
?   Trader.Infrastructure
?   ?   ReadMe.txt
?   ?   Trader.Infrastructure.csproj
?   ?   Brokers
?   ?   ?   BrokerClientFactory.cs
?   ?   ?   EasyTrader
?   ?   ?   ?   EasyTraderBrokerRegistration.cs
?   ?   ?   ?   Capabilities
?   ?   ?   ?   ?   EasyTraderAccountReader.cs
?   ?   ?   ?   ?   EasyTraderAnalysisReader.cs
?   ?   ?   ?   ?   EasyTraderFinanceCommands.cs
?   ?   ?   ?   ?   EasyTraderMarketDataReader.cs
?   ?   ?   ?   ?   EasyTraderOrderHistoryReader.cs
?   ?   ?   ?   ?   EasyTraderSessionManager.cs
?   ?   ?   ?   ?   EasyTraderTradingCommands.cs
?   ?   ?   ?   ?   EasyTraderWatchlistReader.cs
?   ?   ?   ?   Internal
?   ?   ?   ?   ?   EasyTraderCapabilityBase.cs
?   ?   ?   ?   ?   EasyTraderDateParsers.cs
?   ?   ?   ?   ?   EasyTraderNumericParsers.cs
?   ?   ?   ?   ?   EasyTraderOrderHistoryMapper.cs
?   ?   ?   ?   ?   EasyTraderTextHelpers.cs
?   ?   Configurations
?   ?   ?   ExecutionLogConfiguration.cs
?   ?   ?   ScheduledOrderConfiguration.cs
?   ?   ?   SchedulePlanConfiguration.cs
?   ?   ?   TraderAccountConfiguration.cs
?   ?   ?   TraderSymbolConfiguration.cs
?   ?   Data
?   ?   ?   TraderDbContext.cs
?   ?   ?   TraderDbContextFactory.cs
?   ?   ?   TraderSeedData.cs
?   ?   DependencyInjection
?   ?   ?   ApplicationBuilderExtensions.cs
?   ?   ?   HealthCheckExtensions.cs
?   ?   ?   ModuleInitializer.cs
?   ?   ?   ServiceCollectionExtensions.cs
?   ?   Migrations
?   ?   ?   20260925160600_Edit_1_Trader.cs
?   ?   ?   20260925160600_Edit_1_Trader.Designer.cs
?   ?   ?   20260927084217_Edit_2_Trader.cs
?   ?   ?   20260927084217_Edit_2_Trader.Designer.cs
?   ?   ?   TraderDbContextModelSnapshot.cs
?   ?   Scheduler
?   ?   ?   ClockSyncBackgroundService.cs
?   ?   ?   ClockSyncOptions.cs
?   ?   ?   PlanExecutionJobHandler.cs
?   ?   ?   PlanExecutor.cs
?   ?   Services
?   ?   ?   AccountService.cs
?   ?   ?   ExecutionLogService.cs
?   ?   ?   SchedulePlanService.cs
?   ?   ?   ServerClockService.cs
?   ?   ?   SymbolService.cs
?   Trader.Presentation
?   ?   Trader.Presentation.csproj
?   ?   Controllers
?   ?   ?   AccountController.cs
?   ?   ?   ExecutionLogController.cs
?   ?   ?   SchedulePlanController.cs
?   ?   ?   ServerClockController.cs
?   ?   ?   SymbolController.cs
?   ?   DependencyInjection
?   ?   ?   ServiceCollectionExtensions.cs

هر کدوم رو نیاز داری بدونی بگو کدش رو برات بفرستم.