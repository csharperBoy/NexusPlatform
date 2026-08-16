using Core.Shared.Results;
using HR.Application.Interfaces;
using HR.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Commands.Post
{
    public record UpdatePostContactCommand(
      Guid Id,
      string? OfficePhone,
      string? OrgEmail,
      string? OrgMobile

) : IRequest<Result<Guid>>;


    public class UpdatePostContactCommandHandler : IRequestHandler<UpdatePostContactCommand, Result<Guid>>
    {
        private readonly IPostInternalService _orgChartService;
        private readonly ILogger<UpdatePostContactCommandHandler> _logger;

        public UpdatePostContactCommandHandler(
            IPostInternalService orgChartService,
            ILogger<UpdatePostContactCommandHandler> logger)
        {
            _orgChartService = orgChartService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(UpdatePostContactCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Updating PostContact : {id}",
                    request.Id);

                Guid postId = await _orgChartService.UpdatePostAsync(
                    request.Id,
                      null,
                      null,
                      null,
                      null,
                      null,
                      null,
                      null,
                      null,
                       request.OfficePhone, request.OrgEmail, request.OrgMobile
                    );
                

                await _orgChartService.SaveAsync();
                _logger.LogInformation(
                    "PostContact Update successfully: {postId}",
                    postId);

                return Result<Guid>.Ok(postId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create Post: {id}",
                     request.Id);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }


}
