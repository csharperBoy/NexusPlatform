using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Entities
{
    public class OrganizationUnitMap : BaseEntity
    {
        public string? OrganizationUnit { get; set; }
        public Guid? FkOrganizationUnitId { get; set; }
        public decimal? IrisaOrganizationUnitId { get; set; }
        public string? IrisaOrganizationUnit { get; set; }

        //public virtual Post Post { get; private set; } = null!;
        protected OrganizationUnitMap()
        {

        }
        public OrganizationUnitMap(
            string? _OrganizationUnit,
            Guid? _FkOrganizationUnitId,
                decimal? _IrisaOrganizationUnitId,
                string? _IrisaOrganizationUnit
            )
        {
            OrganizationUnit = _OrganizationUnit;
            FkOrganizationUnitId = _FkOrganizationUnitId;
            IrisaOrganizationUnitId = _IrisaOrganizationUnitId;
            IrisaOrganizationUnit = _IrisaOrganizationUnit;
        }
        public OrganizationUnitMap(
                 decimal? _IrisaOrganizationUnitId,
                string? _IrisaOrganizationUnit
            )
        {
            OrganizationUnit = null;
            FkOrganizationUnitId = null;
            IrisaOrganizationUnitId = _IrisaOrganizationUnitId;
            IrisaOrganizationUnit = _IrisaOrganizationUnit;
        }
    }

}
