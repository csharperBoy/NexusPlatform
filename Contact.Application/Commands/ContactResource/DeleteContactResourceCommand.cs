using Contact.Application.Interfaces;
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
   
    public record DeleteContactResourceCommand(Guid Id) : IRequest<Result<bool>>;


    public class DeleteContactResourceCommandHandler : IRequestHandler<DeleteContactResourceCommand, Result<bool>>
    {
        private readonly IContactResourceCommandService _contactResourceService;
        private readonly IPostInternalService _orgChartService;
        private readonly ILogger<DeleteContactResourceCommandHandler> _logger;

        public DeleteContactResourceCommandHandler(
            IContactResourceCommandService contactResourceService,
            ILogger<DeleteContactResourceCommandHandler> logger,
            IPostInternalService orgChartService)
        {
            _contactResourceService = contactResourceService;
            _logger = logger;
            _orgChartService = orgChartService;
        }

        public async Task<Result<bool>> Handle(DeleteContactResourceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Delete ContactResource: {Id}", request.Id);

                await _contactResourceService.DeleteContactResourceAsync(request);

                await _contactResourceService.SaveAsync();

                _logger.LogInformation("ContactResource Deleted successfully: {ContactResourceId}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to Delete ContactResource: {Id}",
                     request.Id);

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
