using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using StreamBus.Application.Abstractions;
using StreamBus.Domain.Enums;
using StreamBus.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StreamBus.Infrastructure.Services
{

    public class GrpcStreamBusClient<TRequest, TResponse> : IStreamBusClient<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        private readonly ILogger<GrpcStreamBusClient<TRequest, TResponse>> _logger;
        private GrpcChannel? _channel;
        private AsyncDuplexStreamingCall<TRequest, TResponse>? _streamingCall;

        public StreamConnectionState State { get; private set; } = StreamConnectionState.Disconnected;

        public GrpcStreamBusClient(ILogger<GrpcStreamBusClient<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public Task ConnectAsync(string address, Method<TRequest, TResponse> grpcMethod, CancellationToken cancellationToken = default)
        {
            try
            {
                State = StreamConnectionState.Connecting;

                // تنظیمات پیشرفته کانال (می‌توانید از appsettings هم بخوانید)
                var channelOptions = new GrpcChannelOptions
                {
                    MaxReceiveMessageSize = null, // نامحدود
                    MaxSendMessageSize = null
                };

                _channel = GrpcChannel.ForAddress(address, channelOptions);

                var callInvoker = _channel.CreateCallInvoker();

                // ایجاد یک استریم دوطرفه روی متد معرفی شده
                _streamingCall = callInvoker.AsyncDuplexStreamingCall(grpcMethod, null, new CallOptions(cancellationToken: cancellationToken));

                State = StreamConnectionState.Connected;
                _logger.LogInformation("Successfully connected StreamBus to {Address} via {MethodName}", address, grpcMethod.Name);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                State = StreamConnectionState.Faulted;
                _logger.LogError(ex, "Failed to connect StreamBus to {Address}", address);
                throw new StreamBusException("Could not establish gRPC connection.", ex);
            }
        }

        public async Task SendAsync(TRequest message, CancellationToken cancellationToken = default)
        {
            if (_streamingCall == null || State != StreamConnectionState.Connected)
                throw new StreamBusException("Stream is not connected. Call ConnectAsync first.");

            try
            {
                await _streamingCall.RequestStream.WriteAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                State = StreamConnectionState.Faulted;
                _logger.LogError(ex, "Error sending message over stream.");
                throw;
            }
        }

        public async IAsyncEnumerable<TResponse> ReceiveAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_streamingCall == null || State != StreamConnectionState.Connected)
                throw new StreamBusException("Stream is not connected. Call ConnectAsync first.");

            // استفاده از قابلیت IAsyncStreamReader دات‌نت برای خواندن به صورت IAsyncEnumerable
            await foreach (var message in _streamingCall.ResponseStream.ReadAllAsync(cancellationToken))
            {
                yield return message;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_streamingCall != null)
            {
                try
                {
                    // اعلام پایان ارسال به سرور
                    await _streamingCall.RequestStream.CompleteAsync();
                    _streamingCall.Dispose();
                }
                catch { /* نادیده گرفتن خطای زمان قطع شدن */ }
            }

            if (_channel != null)
            {
                _channel.Dispose(); 
            }

            State = StreamConnectionState.Disconnected;
            _logger.LogInformation("StreamBus connection closed and disposed.");
        }
    }
}
