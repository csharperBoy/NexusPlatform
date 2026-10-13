using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Brokers
{
        /// <summary>پایه‌ی همه‌ی اینترفیس‌های بروکر. فقط BrokerType رو می‌شناسه.</summary>
        public interface IBroker
        {
            BrokerType BrokerType { get; }
        }
    
}
