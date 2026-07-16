using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Repositories;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Infrastructure.Data;
using HR.IrisaSync.Extention.Data;
using HR.IrisaSync.Extention.Entities;
using HR.IrisaSync.Extention.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Services
{
    public class IrisaSyncUnitOfWork : EfUnitOfWork<IrisaExtentionDbContext>, IIrisaSyncUnitOfWork<IrisaExtentionDbContext>
    {
        public IrisaSyncUnitOfWork(
            IRepository<IrisaExtentionDbContext, IrisaSyncJobTitleMap, Guid> jobTitleMapRepository,
            IRepository<IrisaExtentionDbContext, IrisaSyncJobLevelMap, Guid> jobLevelMapRepository,
            IRepository<IrisaExtentionDbContext, IrisaSyncOrganizationUnitMap, Guid> organizationUnitMapRepository,

            IrisaExtentionDbContext dbContext,
            IOutboxService<IrisaExtentionDbContext> outboxService,
            ILogger<EfUnitOfWork<IrisaExtentionDbContext>> logger) : base(dbContext, outboxService, logger)
        {
            JobTitleMapRepository = jobTitleMapRepository;
            JobLevelMapRepository = jobLevelMapRepository;
            OrganizationUnitMapRepository = organizationUnitMapRepository;


        }

      public IRepository<IrisaExtentionDbContext, IrisaSyncJobTitleMap, Guid> JobTitleMapRepository { get; }
      public IRepository<IrisaExtentionDbContext, IrisaSyncJobLevelMap, Guid> JobLevelMapRepository { get; }
        public IRepository<IrisaExtentionDbContext, IrisaSyncOrganizationUnitMap, Guid> OrganizationUnitMapRepository { get; }
    }
}
