using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Common
{
    public interface IDataScopedEntity
    {
        // مشخص می‌کند این رکورد متعلق به کدام واحد است (برای اسکوپ‌های Unit و UnitAndBelow)
        Guid? OwnerOrganizationUnitId { get; }

        // مشخص می‌کند این رکورد متعلق به کدام پست سازمانی است (برای اسکوپ Self)
        Guid? OwnerPositionId { get; }

        // مشخص می‌کند این رکورد متعلق به کدام شخص است (برای اسکوپ Self)
        Guid? OwnerPersonId { get; }
    }

    public abstract class DataScopedEntity : AuditableEntity, IDataScopedEntity
    {
        public Guid? OwnerOrganizationUnitId { get; protected set; }
        public Guid? OwnerPositionId { get; protected set; }
        public Guid? OwnerPersonId { get; protected set; }

        public void SetOwners(Guid? personId, Guid? positiontId, Guid? orgUnitId)
        {
            OwnerPersonId = personId;
            OwnerPositionId = positiontId;
            OwnerOrganizationUnitId = orgUnitId;
        }
        public void SetPersonOwner(Guid personId)
        {
            OwnerPersonId = personId;
        }
        public void SetPositionOwner(Guid positiontId)
        {
            OwnerPositionId = positiontId;
        }
        public void SetOrganizationUnitOwner(Guid orgUnitId)
        {
            OwnerOrganizationUnitId = orgUnitId;
        }
    }
}
