using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Commands.Post
{
    public record BatchUpdatePostsContactCommand(List<UpdatePostContactCommand> Posts) : IRequest<Result<List<Guid>>>;

    
    public class BatchUpdatePostsContactCommandHandler : IRequestHandler<BatchUpdatePostsContactCommand, Result<List<Guid>>>
    {
        private readonly IPostInternalService _orgChartService;
        private readonly ILogger<BatchUpdatePostsContactCommandHandler> _logger;

        public BatchUpdatePostsContactCommandHandler(IPostInternalService orgChartService, ILogger<BatchUpdatePostsContactCommandHandler> logger)
        {
            _orgChartService = orgChartService;
            _logger = logger;
        }

        public async Task<Result<List<Guid>>> Handle(BatchUpdatePostsContactCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var results = new List<Guid>();
                foreach (var command in request.Posts)
                {
                    // ۱. به‌روزرسانی اطلاعات پایه پست
                    Guid postId = await _orgChartService.UpdatePostAsync(
                        command.Id,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        command.OfficePhone,
                        command.OrgEmail,
                        command.OrgMobile
                    );


                    results.Add(postId);
                }

                // ۳. ذخیره‌سازی یکباره همه تغییرات
                await _orgChartService.SaveAsync();

                _logger.LogInformation("Batch update of {count} posts completed successfully.", results.Count);
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
