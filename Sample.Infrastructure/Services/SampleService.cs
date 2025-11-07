using Azure;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Auditing;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.Security;
using Core.Domain.Events;
using Core.Infrastructure.Repositories;
using Core.Shared.Results;
using Microsoft.Extensions.Logging;
using Sample.Application.DTOs;
using Sample.Application.Interfaces;
using Sample.Domain.Entities;
using Sample.Domain.Events;
using Sample.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sample.Infrastructure.Services
{
    public class SampleService : ISampleService
    {

        private readonly ISpecificationRepository<SampleEntity, Guid> _specRepository;
        private readonly IRepository<SampleDbContext, SampleEntity, Guid> _repository;
        private readonly IUnitOfWork<SampleDbContext> _uow;
        private readonly ILogger<SampleService> _logger;

        private readonly ICurrentUserService _currentUser;
        private readonly IOutboxService<SampleDbContext> _outboxService;
        public SampleService(IRepository<SampleDbContext,
            SampleEntity, Guid> repository,
            IUnitOfWork<SampleDbContext> uow,
            ILogger<SampleService> logger,
            ISpecificationRepository<SampleEntity, Guid> specRepository,
            IOutboxService<SampleDbContext> outboxService,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _uow = uow;
            _logger = logger;
            _specRepository = specRepository;
            _outboxService = outboxService;
            _currentUser = currentUser;
        }

        public async Task<Result<SampleApiResponse>> SampleApiMethodAsync(SampleApiRequest request)
        {
            var userId = _currentUser.UserId;

            var entity = new SampleEntity();
            entity.MarkSample(request.property1);

            await _repository.AddAsync(entity);
            await _uow.SaveChangesAsync();

            var sampleEvent = new SampleActionEvent(request.property1);
            await _outboxService.AddEventsAsync(new[] { sampleEvent });

            return Result<SampleApiResponse>.Ok(new SampleApiResponse("ok"));
        }

        public Task<Result<SampleApiResponse>> sampleEventHandlerMethodAsync(object property1)
        {
            throw new NotImplementedException();
        }


    }
}