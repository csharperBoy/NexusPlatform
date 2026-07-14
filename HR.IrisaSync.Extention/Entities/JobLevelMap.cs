using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Entities
{
  
    public class JobLevelMap : BaseEntity
    {
        public string? JobLevel { get; set; }
        public Guid? FkJobLevelId { get; set; }
        public decimal? IrisaJobLevelId { get; set; }
        public string? IrisaJobLevel { get; set; }

        //public virtual Post Post { get; private set; } = null!;
        protected JobLevelMap()
        {

        }
        public JobLevelMap(
            string? _JobLevel,
            Guid? _FkJobLevelId,
                decimal? _IrisaJobLevelId,
                string? _IrisaJobLevel
            )
        {
            JobLevel = _JobLevel;
            FkJobLevelId = _FkJobLevelId;
            IrisaJobLevelId = _IrisaJobLevelId;
            IrisaJobLevel = _IrisaJobLevel;
        }
        public JobLevelMap(
                 decimal? _IrisaJobLevelId,
                string? _IrisaJobLevel
            )
        {
            JobLevel = null;
            FkJobLevelId = null;
            IrisaJobLevelId = _IrisaJobLevelId;
            IrisaJobLevel = _IrisaJobLevel;
        }
    }

}
