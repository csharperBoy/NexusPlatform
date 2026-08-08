using Grpc.Core;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using StreamBus.Application.Abstractions;
using StreamBus.Domain.Enums;
using System.Runtime.CompilerServices;

namespace StreamBus.Infrastructure.Services
{
    
    public class ResilientStreamBusClient<TRequest, TResponse> : IStreamBusClient<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        private readonly GrpcStreamBusClient<TRequest, TResponse> _innerClient;
        private readonly ILogger<ResilientStreamBusClient<TRequest, TResponse>> _logger;
        private readonly AsyncRetryPolicy _reconnectPolicy;

        private string? _lastAddress;
        private Method<TRequest, TResponse>? _lastMethod;

        public StreamConnectionState State => _innerClient.State;

        public ResilientStreamBusClient(
            ILogger<GrpcStreamBusClient<TRequest, TResponse>> innerLogger,
            ILogger<ResilientStreamBusClient<TRequest, TResponse>> logger)
        {
            _innerClient = new GrpcStreamBusClient<TRequest, TResponse>(innerLogger);
            _logger = logger;

            // 📌 تعریف سیاست Retry اختصاصی برای قطعی‌های شبکه استریم با Exponential Backoff
            _reconnectPolicy = Policy
                .Handle<RpcException>()
                .Or<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Stream connection lost. Reconnecting attempt {RetryCount} after {Delay}s...",
                            retryCount,
                            timeSpan.TotalSeconds);
                    });
        }

        public async Task ConnectAsync(string address, Method<TRequest, TResponse> grpcMethod, CancellationToken cancellationToken = default)
        {
            _lastAddress = address;
            _lastMethod = grpcMethod;

            await _reconnectPolicy.ExecuteAsync(async (ct) =>
            {
                await _innerClient.ConnectAsync(address, grpcMethod, ct);
            }, cancellationToken);
        }

        public async Task SendAsync(TRequest message, CancellationToken cancellationToken = default)
        {
            // ارسال پیام با سیاست مجدد در صورت خطای موقت شبکه
            await _reconnectPolicy.ExecuteAsync(async (ct) =>
            {
                await _innerClient.SendAsync(message, ct);
            }, cancellationToken);
        }

        public async IAsyncEnumerable<TResponse> ReceiveAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                IAsyncEnumerator<TResponse>? enumerator = null;

                try
                {
                    enumerator = _innerClient.ReceiveAllAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize stream reader. Retrying connection...");
                }

                if (enumerator == null)
                {
                    await TryReconnectAsync(cancellationToken);
                    continue;
                }

                while (!cancellationToken.IsCancellationRequested)
                {
                    bool hasNext;
                    try
                    {
                        hasNext = await enumerator.MoveNextAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Stream connection interrupted during read. Attempting reconnect...");
                        break; // خروج از حلقه داخلی جهت اتصال مجدد
                    }

                    if (!hasNext) break;

                    yield return enumerator.Current;
                }

                await enumerator.DisposeAsync();

                // اگر استریم قطع شد و لغو نشده بود، سعی در Reconnect می‌کند
                if (!cancellationToken.IsCancellationRequested)
                {
                    await TryReconnectAsync(cancellationToken);
                }
            }
        }

        private async Task TryReconnectAsync(CancellationToken cancellationToken)
        {
            if (_lastAddress != null && _lastMethod != null)
            {
                try
                {
                    await _reconnectPolicy.ExecuteAsync(async (ct) =>
                    {
                        await _innerClient.ConnectAsync(_lastAddress, _lastMethod, ct);
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "All reconnect attempts failed.");
                    throw;
                }
            }
        }

        public ValueTask DisposeAsync() => _innerClient.DisposeAsync();
    }
}
