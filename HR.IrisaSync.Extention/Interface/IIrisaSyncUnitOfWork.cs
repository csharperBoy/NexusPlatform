using Core.Application.Abstractions;
using HR.Domain.Entities;
using HR.IrisaSync.Extention.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Interface
{
    public interface IIrisaSyncUnitOfWork<TContext> : IUnitOfWork<TContext>
        where TContext : DbContext
    {
        IRepository<TContext, JobTitleMap, Guid> JobTitleMapRepository { get; }
        IRepository<TContext, JobLevelMap, Guid> JobLevelMapRepository { get; }
        IRepository<TContext, OrganizationUnitMap, Guid> OrganizationUnitMapRepository { get; }

    }
}
