using Grpc.Core;
using StreamBus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamBus.Application.Abstractions
{
    public interface IStreamBusClient<TRequest, TResponse> : IAsyncDisposable
    where TRequest : class
    where TResponse : class
    {
        StreamConnectionState State { get; }

        /// <summary>
        /// اتصال به سرور gRPC به صورت کاملاً ژنریک
        /// </summary>
        Task ConnectAsync(string address, Method<TRequest, TResponse> grpcMethod, CancellationToken cancellationToken = default);

        /// <summary>
        /// ارسال یک پیام روی استریم باز
        /// </summary>
        Task SendAsync(TRequest message, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت مداوم پیام‌ها از سرور به صورت Async Stream
        /// </summary>
        IAsyncEnumerable<TResponse> ReceiveAllAsync(CancellationToken cancellationToken = default);
    }
}
