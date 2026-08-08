
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StreamBus.Application.Abstractions;
using StreamBus.Application.Notifications;
using StreamBus.Application.Options;

namespace StreamBus.Infrastructure.BackgroundServices
{

    public class StreamBusConsumerWorker<TRequest, TResponse> : BackgroundService
        where TRequest : class
        where TResponse : class
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly StreamConsumerOptions<TRequest, TResponse> _options;
        private readonly ILogger<StreamBusConsumerWorker<TRequest, TResponse>> _logger;

        public StreamBusConsumerWorker(
            IServiceScopeFactory scopeFactory,
            StreamConsumerOptions<TRequest, TResponse> options,
            ILogger<StreamBusConsumerWorker<TRequest, TResponse>> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting StreamBus Consumer Worker for {ResponseType}...", typeof(TResponse).Name);

            if (string.IsNullOrEmpty(_options.ServerAddress) || _options.GrpcMethod == null)
            {
                _logger.LogError("StreamBus Consumer configuration is incomplete. Worker stopping.");
                return;
            }

            // ایجاد یک Scope اختصاصی برای کلاینت استریم
            using var streamScope = _scopeFactory.CreateScope();
            var streamClient = streamScope.ServiceProvider.GetRequiredService<IStreamBusClient<TRequest, TResponse>>();

            try
            {
                // ۱. برقراری اتصال به استریم
                await streamClient.ConnectAsync(_options.ServerAddress, _options.GrpcMethod, stoppingToken);

                // ۲. ارسال پیام اولیه/دستور ثبت‌نام در صورت وجود
                if (_options.InitialRequest != null)
                {
                    await streamClient.SendAsync(_options.InitialRequest, stoppingToken);
                }

                // ۳. دریافت مداوم پیام‌ها و ارسال به MediatR
                await foreach (var message in streamClient.ReceiveAllAsync(stoppingToken))
                {
                    await ProcessMessageAsync(message, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("StreamBus Consumer Worker is stopping gracefully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error occurred in StreamBus Consumer Worker.");
            }
        }

        private async Task ProcessMessageAsync(TResponse message, CancellationToken cancellationToken)
        {
            // 📌 ایجاد یک Scope مجزا برای هر پیام جهت مدیریت صحیح Lifetime سرویس‌های Scoped (مثل DbContext و Handlerها)
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            try
            {
                var notification = new StreamMessageNotification<TResponse>(message);
                await mediator.Publish(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                // خطای پردازش یک پیام نباید کل حلقه استریم را متوقف کند
                _logger.LogError(ex, "Error occurred while processing message of type {MessageType} via MediatR.", typeof(TResponse).Name);
            }
        }
    }
}
