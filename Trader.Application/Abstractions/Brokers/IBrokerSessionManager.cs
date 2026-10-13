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
