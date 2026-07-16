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

        Task FillOrganizationUnitMap();
        //Guid GetPostId(decimal? codJobpo);
    }
}
