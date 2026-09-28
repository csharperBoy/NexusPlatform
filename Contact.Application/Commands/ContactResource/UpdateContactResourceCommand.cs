using Contact.Application.Interfaces;
using Contact.Domain.Enums;
using Core.Domain.Common;
using Core.Shared.Enums.Contact;
using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Commands.ContactResource
{
    
    public record UpdateContactResourceCommand(
           Guid Id,
   Optional<string> Value,
   Optional<string?> Label,
   Optional<ContactTypeEnum> ContactType,
   Optional<bool> IsPrimary,
   Optional<int?> SortOrder,
   Optional<ContactRelationTypeEnum?> RelationType,
   Optional<Guid?> ParentId


) : IRequest<Result<Guid>>;


    public class UpdateContactResourceCommandHandler : IRequestHandler<UpdateContactResourceCommand, Result<Guid>>
    {
        private readonly IContactResourceCommandService _contactResourceService;
        private readonly ILogger<UpdateContactResourceCommandHandler> _logger;

        public UpdateContactResourceCommandHandler(
            IContactResourceCommandService contactResourceService,
            ILogger<UpdateContactResourceCommandHandler> logger)
        {
            _contactResourceService = contactResourceService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(UpdateContactResourceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating contactResource: {Value}",
                    request.Value);

                bool hasChange = await _contactResourceService.UpdateContactResourceAsync(request);
              //bool hasChange =  await _contactResourceService.UpdateContactResourceAsync(
              //      request.Id,
              //      request.Value,
              //      request.Label,
              //      request.ContactType,
              //      request.EffectiveFrom,
              //      request.IsPrimary,
              //      request.SortOrder,
              //      request.ParentContactResourceId,
              //      request.RelationType,
              //      request.ParentId
              //      );
                Guid ContactResourceId = request.Id;
                await _contactResourceService.SaveAsync();
                _logger.LogInformation(
                    "ContactResource created successfully: {Id} ({Value})",
                    ContactResourceId, request.Value);

                return Result<Guid>.Ok(ContactResourceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create ContactResource: {Value}",
                     request.Value);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}
