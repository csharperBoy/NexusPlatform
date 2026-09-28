using Contact.Application.Interfaces;
using Core.Domain.Common;
using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Commands.ContactResource
{
    public record BatchUpdateContactResourcesCommand(List<UpdateContactResourceCommand> ContactResources) : IRequest<Result<List<Guid>>>;


    public class BatchUpdateContactResourcesCommandHandler : IRequestHandler<BatchUpdateContactResourcesCommand, Result<List<Guid>>>
    {
        private readonly IContactResourceCommandService _contactResourceService;
        private readonly ILogger<BatchUpdateContactResourcesCommandHandler> _logger;

        public BatchUpdateContactResourcesCommandHandler( ILogger<BatchUpdateContactResourcesCommandHandler> logger, IContactResourceCommandService contactResourceService)
        {
            _logger = logger;
            _contactResourceService = contactResourceService;
        }

        public async Task<Result<List<Guid>>> Handle(BatchUpdateContactResourcesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var results = new List<Guid>();
                foreach (var command in request.ContactResources)
                {
                    // ۱. به‌روزرسانی اطلاعات پایه پست

                    bool hasChange = await _contactResourceService.UpdateContactResourceAsync(command);
                    //bool hasChange = await _contactResourceService.UpdateContactResourceAsync(
                    // command.Id,
                    // command.Value,
                    // command.Label,
                    // command.ContactType,
                    // command.EffectiveFrom,
                    // command.IsPrimary,
                    // command.SortOrder,
                    // command.ParentContactResourceId,
                    // command.RelationType,
                    // command.ParentId
                    // );

                    Guid ContactResourceId = command.Id;
                   results.Add(ContactResourceId);
                }

                await _contactResourceService.SaveAsync();

                _logger.LogInformation("Batch update of {count} ContactResource completed successfully.", results.Count);
                return Result<List<Guid>>.Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to batch update posts.");
                return Result<List<Guid>>.Fail(ex.Message);
            }
        }
    }
}
