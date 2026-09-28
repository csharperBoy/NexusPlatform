using Contact.Application.Interfaces;
using Contact.Domain.Enums;
using Core.Application.Abstractions.People;
using Core.Application.Context;
using Core.Application.Provider;
using Core.Domain.Common;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;
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
    public record CreateContactResourceCommand(
   string Value,
   string? Label,
   ContactTypeEnum ContactType,
   bool IsPrimary,
   int? SortOrder,
   ContactRelationTypeEnum? RelationType,
   Guid? ParentId


) : IRequest<Result<Guid>>;


   

    public class CreateContactResourceCommandHandler : IRequestHandler<CreateContactResourceCommand, Result<Guid>>
    {
        private readonly IContactResourceCommandService _contactResourceService;
        private readonly ILogger<CreateContactResourceCommandHandler> _logger;

        public CreateContactResourceCommandHandler(
            IContactResourceCommandService contactResourceService,
            ILogger<CreateContactResourceCommandHandler> logger)
        {
            _contactResourceService = contactResourceService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreateContactResourceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating ContactResource: {Value}",
                    request.Value);

                Guid contactResourceId = await _contactResourceService.CreateContactResourceAsync(request);
                     
              

                await _contactResourceService.SaveAsync();
                _logger.LogInformation(
                    "ContactResource created successfully: {contactResourceId} ({Value})",
                    contactResourceId, request.Value);

                return Result<Guid>.Ok(contactResourceId);
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
