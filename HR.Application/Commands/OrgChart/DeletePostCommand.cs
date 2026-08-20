using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.OrgChart
{
   

    public record DeletePostCommand(Guid Id) : IRequest<Result<bool>>;


    public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, Result<bool>>
    {
        private readonly IPostInternalService _postService;
        private readonly ILogger<DeletePostCommandHandler> _logger;

        public DeletePostCommandHandler(
            IPostInternalService postService,
            ILogger<DeletePostCommandHandler> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeletePostCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Delete Post: {Id}", request.Id);

                await _postService.DeleteAsync(request.Id);

                await _postService.SaveAsync();

                _logger.LogInformation("Post Deleted successfully: {PostId}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to Delete Post: {Id}",
                     request.Id);

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
