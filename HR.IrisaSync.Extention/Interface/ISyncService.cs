using HR.IrisaSync.Extention.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Interface
{
    public interface ISyncService
    {
        Task SyncEmployements();
        Task<IReadOnlyList<PdsIdeaInformationViw>> GetEmployee();

        Task SyncJobTitle();

        Task SyncJobLevel();

        Task SyncOrganizationUnit();

        Task SyncPost();
    }
}
