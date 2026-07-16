using Core.Domain.Common.EntityProperties;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Entities
{
    public class IrisaSyncJobTitleMap : BaseEntity
    {
        public string? JobTitle { get; set; }
        public Guid? FkJobTitleId { get; set; }
        public decimal? IrisaJobTitleId { get; set; }
        public string? IrisaJobTitle { get; set; }
        public int? IrisaJobTitleUseCount { get; set; }

        //public virtual Post Post { get; private set; } = null!;
        protected IrisaSyncJobTitleMap()
        {
            
        }
        public IrisaSyncJobTitleMap(
            string? _JobTitle,
            Guid? _FkJobTitleId,
                decimal? _IrisaJobTitleId,
                string? _IrisaJobTitle,
                int? _IrisaJobTitleUseCount
            )
        {
            JobTitle = _JobTitle;
            FkJobTitleId = _FkJobTitleId;
            IrisaJobTitleId = _IrisaJobTitleId;
            IrisaJobTitle = _IrisaJobTitle;
            IrisaJobTitleUseCount = _IrisaJobTitleUseCount;
        }
        public IrisaSyncJobTitleMap(
                 decimal? _IrisaJobTitleId,
                string? _IrisaJobTitle, int? _IrisaJobTitleUseCount
            )
        {
            JobTitle = null;
            FkJobTitleId = null;
            IrisaJobTitleId = _IrisaJobTitleId;
            IrisaJobTitle = _IrisaJobTitle;
            IrisaJobTitleUseCount = _IrisaJobTitleUseCount;
        }
    }
}
