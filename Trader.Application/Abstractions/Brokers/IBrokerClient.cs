using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Application.Abstractions.Brokers.Commands;
using Trader.Application.Abstractions.Brokers.Queries;

namespace Trader.Application.Abstractions.Brokers
{
        /// <summary>
        /// نمای کلی بروکر. برای جاهایی که نیاز به چند دسته دارند.
        /// ترجیحاً در مصرف‌کننده‌ها از اینترفیس‌های کوچک‌تر استفاده کن.
        /// </summary>
        public interface IBrokerClient :
            IBrokerSessionManager,
            IBrokerTradingCommands,
            IBrokerFinanceCommands,
            IBrokerMarketDataReader,
            IBrokerAccountReader,
            IBrokerOrderHistoryReader,
            IBrokerAnalysisReader,
            IBrokerWatchlistReader
        {
        }
    
}
