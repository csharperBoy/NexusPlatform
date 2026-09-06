using Core.Shared.Results;
using HR.IrisaSync.Extention.Entities;
using HR.IrisaSync.Extention.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Interface
{
    public interface ISyncService
    {
        Task<BatchResult<SyncResult>> SyncEmploymentsAsync();
        Task<IReadOnlyList<PdsIdeaInformationViw>> GetEmployment();

        Task<BatchResult<SyncResult>> SyncJobTitleAsync();

        Task<BatchResult<SyncResult>> SyncJobLevelAsync();

        Task<BatchResult<SyncResult>> SyncOrganizationUnitAsync();

        Task<BatchResult<SyncResult>> SyncPostAsync();
        Task<BatchResult< SyncResult>> SyncAssignmentsAsync();
    }
}
