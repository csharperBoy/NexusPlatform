using Core.Application.Abstractions.People;
using Core.Application.Context;
using Core.Application.Provider;
using Core.Domain.ValueObjects;
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

namespace HR.Application.Commands.JobLevel
{
    public record CreateJobLevelCommand(
            string Code,
            string Title
        ) : IRequest<Result<Guid>>;


    public class CreateJobLevelCommandHandler : IRequestHandler<CreateJobLevelCommand, Result<Guid>>
    {
        private readonly IJobLevelInternalService _service;
        private readonly ILogger<CreateJobLevelCommandHandler> _logger;
        private readonly IUserDataContextProvider _userProvider;
        public CreateJobLevelCommandHandler(
            IJobLevelInternalService service,
           IUserDataContextProvider userProvider,
        ILogger<CreateJobLevelCommandHandler> logger)
        {
            _service = service;
            _logger = logger;
            _userProvider = userProvider;
        }

        public async Task<Result<Guid>> Handle(CreateJobLevelCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    $"Creating JobLevel: {request.Title}");
                UserDataContext userContext = await _userProvider.GetAsync(new CancellationToken());


                Guid newId = await _service.CreateAsync(request.Code,
                    request.Title
                  , userContext.UserName
                    );

                await _service.SaveAsync();
                _logger.LogInformation(
                    $"JobLevel created successfully: {request.Title}");

                return Result<Guid>.Ok(newId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Failed to create Post: {request.Title}");

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }

}
