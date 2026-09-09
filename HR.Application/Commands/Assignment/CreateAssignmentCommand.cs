using Core.Application.Context;
using Core.Application.Provider;
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

namespace HR.Application.Commands.Assignment
{
    public record CreateAssignmentCommand(
           Guid employmentId,
           List<Guid?> postIds,
            PostAssignmentType? assigneType = null,
            DateTime? effectiveFrom = null,
            DateTime? effectiveTo = null

       ) : IRequest<Result<bool>>;


    public class CreateAssignmentCommandHandler : IRequestHandler<CreateAssignmentCommand, Result<bool>>
    {
        private readonly IPostInternalService _postService;
        private readonly ILogger<CreateAssignmentCommandHandler> _logger;
        private readonly IUserDataContextProvider _userProvider;
        public CreateAssignmentCommandHandler(
            IPostInternalService postService,
           IUserDataContextProvider userProvider,
        ILogger<CreateAssignmentCommandHandler> logger)
        {
            _postService = postService;
            _logger = logger;
            _userProvider = userProvider;
        }

        public async Task<Result<bool>> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    $"Creating Assignment");
                UserDataContext userContext = await _userProvider.GetAsync(new CancellationToken());


                bool hasChange = await _postService.AssignToEmploymentAsync(request.postIds,
                    request.employmentId,request.assigneType , request.effectiveFrom , request.effectiveTo
                    );

                await _postService.SaveAsync();
                _logger.LogInformation(
                    $"Assignment created successfully");

                return Result<bool>.Ok(hasChange);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Failed to create Post Assign");

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
