using Core.Application.Abstractions.Security;
using Core.Shared.Results;
using MediatR;
using Sample.Application.Commands;
using Sample.Application.DTOs;
using Sample.Application.Interfaces;

namespace Sample.Application.Handlers.Commands
{
    public class SampleApiCommandHandler : IRequestHandler<SampleApiCommand, Result<SampleApiResponse>>
    {

        private readonly ISampleService _sampleService;
        private readonly IPermissionChecker _permissionChecker;

        public SampleApiCommandHandler(ISampleService sampleService, IPermissionChecker permissionChecker)
        {
            _sampleService = sampleService;
            _permissionChecker = permissionChecker;
        }

        public async Task<Result<SampleApiResponse>> Handle(SampleApiCommand request, CancellationToken cancellationToken)
        {
            var hasPermission = await _permissionChecker.HasPermissionAsync("Sample.Execute");
            if (!hasPermission)
                return Result<SampleApiResponse>.Fail("مجوز لازم وجود ندارد.");

            var result = await _sampleService.SampleApiMethodAsync(new SampleApiRequest(request.property1, request.property2));
            if (!result.Succeeded)
                return Result<SampleApiResponse>.Fail("عملیات با خطا مواجه شد.");

            return Result<SampleApiResponse>.Ok(new SampleApiResponse("عملیات موفق بود"));
        }
    }

}
