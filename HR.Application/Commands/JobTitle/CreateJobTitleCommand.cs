using Core.Application.Context;
using Core.Application.Provider;
using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.JobTitle
{
    public record CreateJobTitleCommand(
           string Code,
           string Name
       ) : IRequest<Result<Guid>>;


    public class CreateJobTitleCommandHandler : IRequestHandler<CreateJobTitleCommand, Result<Guid>>
    {
        private readonly IJobTitleInternalService _service;
        private readonly ILogger<CreateJobTitleCommandHandler> _logger;
        private readonly IUserDataContextProvider _userProvider;
        public CreateJobTitleCommandHandler(
            IJobTitleInternalService service,
           IUserDataContextProvider userProvider,
        ILogger<CreateJobTitleCommandHandler> logger)
        {
            _service = service;
            _logger = logger;
            _userProvider = userProvider;
        }

        public async Task<Result<Guid>> Handle(CreateJobTitleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    $"Creating JobTitle: {request.Name}");
                UserDataContext userContext = await _userProvider.GetAsync(new CancellationToken());


                Guid newId = await _service.CreateAsync(request.Code,
                    request.Name
                  , userContext.UserName
                    );

                await _service.SaveAsync();
                _logger.LogInformation(
                    $"JobTitle created successfully: {request.Name}");

                return Result<Guid>.Ok(newId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Failed to create Post: {request.Name}");

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }

}
