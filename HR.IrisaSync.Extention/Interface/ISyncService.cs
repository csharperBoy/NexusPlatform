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
        Task<SyncResult> SyncEmploymentsAsync();
        Task<IReadOnlyList<PdsIdeaInformationViw>> GetEmployment();

        Task<SyncResult> SyncJobTitleAsync();

        Task<SyncResult> SyncJobLevelAsync();

        Task<SyncResult> SyncOrganizationUnitAsync();

        Task<SyncResult> SyncPostAsync();
        Task<SyncResult> SyncAssignmentsAsync();
    }
}
