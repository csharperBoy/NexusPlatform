using HR.IrisaSync.Extention.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Interface
{
    public interface IMapService
    {
        Task FillJobTitleMap();

        Task FillJobLevelMap();

        Task FillOrganizationUnitRootMap();
        Task FillOrganizationUnitMap();
        Task SyncJobLevelDoneAsync(Guid newId,string? Title, decimal? irisaId);
        Task SyncJobTitleDoneAsync(Guid newId, string? Name, decimal? irisaId);
        Task SyncOrganizationUnitDoneAsync(Guid newId, string? Name, decimal? irisaId);
        Task SaveAsync();
        Task<IrisaSyncOrganizationUnitMap?> GetOrgUnitByIrisaId(decimal? irsiaId);
        //Guid GetPostId(decimal? codJobpo);
    }
}
