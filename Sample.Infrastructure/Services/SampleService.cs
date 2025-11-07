using Core.Application.Abstractions;
using Core.Application.Abstractions.Auditing;
using Core.Infrastructure.Repositories;
using Core.Shared.Results;
using Microsoft.Extensions.Logging;
using Sample.Application.DTOs;
using Sample.Application.Interfaces;
using Sample.Domain.Entities;
using Sample.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Audit.Infrastructure.Services
{
    public class SampleService : ISampleService
    {

        private readonly ISpecificationRepository<SampleEntity, Guid> _specRepository;
        private readonly IRepository<SampleDbContext, SampleEntity, Guid> _repository;
        private readonly IUnitOfWork<SampleDbContext> _uow;
        private readonly ILogger<SampleService> _logger;

        public SampleService(IRepository<SampleDbContext, 
            SampleEntity, Guid> repository, 
            IUnitOfWork<SampleDbContext> uow,
            ILogger<SampleService> logger,
            ISpecificationRepository<SampleEntity, Guid> specRepository)
        {
            _repository = repository;
            _uow = uow;
            _logger = logger;
            _specRepository = specRepository;
        }

        public Task<Result<SampleApiResponse>> SampleApiMethodAsync(SampleApiRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result<SampleApiResponse>> sampleEventHandlerMethodAsync(object property1)
        {
            throw new NotImplementedException();
        }


    }
}