using System.Text.Json;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;
using Scheduler.Domain.ValueObjects;

namespace Scheduler.Application.Commands.RecurringSchedules;

public record CreateRecurringScheduleCommand(
    string Name,
    string JobType,
    string PayloadJson,
    string PatternJson,
    DateTimeOffset? StartAt = null,
    DateTimeOffset? EndAt = null,
    int? MaxOccurrences = null,
    int? MaxFailures = null,
    string? TimeZoneId = null
) : IRequest<Result<Guid>>;

public class CreateRecurringScheduleCommandHandler
    : IRequestHandler<CreateRecurringScheduleCommand, Result<Guid>>
{
    private readonly ISchedulerService _scheduler;
    private readonly IRecurringSchedulePayloadFactory _payloadFactory;
    private readonly ILogger<CreateRecurringScheduleCommandHandler> _logger;

    public CreateRecurringScheduleCommandHandler(
        ISchedulerService scheduler,
        IRecurringSchedulePayloadFactory payloadFactory,
        ILogger<CreateRecurringScheduleCommandHandler> logger)
    {
        _scheduler = scheduler;
        _payloadFactory = payloadFactory;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateRecurringScheduleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var payloadType = _payloadFactory.ResolveType(request.JobType)
                ?? throw new InvalidOperationException(
                    $"Unknown JobType: {request.JobType}");

            var payload = JsonSerializer.Deserialize(request.PayloadJson, payloadType)
                ?? throw new InvalidOperationException("Payload deserialization failed.");

            var pattern = RecurrencePattern.FromJson(request.PatternJson);

            var tz = string.IsNullOrWhiteSpace(request.TimeZoneId)
                ? TimeZoneInfo.FindSystemTimeZoneById(
                    OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran")
                : TimeZoneInfo.FindSystemTimeZoneById(request.TimeZoneId);

            var method = typeof(ISchedulerService)
                .GetMethods()
                .First(m => m.Name == nameof(ISchedulerService.ScheduleRecurringAsync))
                .MakeGenericMethod(payloadType);

            var task = (Task<Guid>)method.Invoke(_scheduler, new object?[]
            {
                request.Name,
                payload,
                pattern,
                request.StartAt,
                request.EndAt,
                request.MaxOccurrences,
                request.MaxFailures ?? 10,
                tz,
                null,
                cancellationToken
            })!;

            var id = await task;
            _logger.LogInformation(
                "RecurringSchedule created: {Id} name={Name} type={Type}",
                id, request.Name, request.JobType);

            return Result<Guid>.Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create RecurringSchedule {Name}", request.Name);
            return Result<Guid>.Fail(ex.Message);
        }
    }
}