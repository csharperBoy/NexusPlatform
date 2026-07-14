using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Common.EntityProperties
{
    /// <summary>
    ///  یعنی موجودیت دارای فیلد های مالک میباشد 
    ///  (OwnerOrganizationUnit - OwnerPosition - OwnerPerson - OwnerUser)
    /// </summary>
    public interface IOwnerableEntity
    {
        // مشخص می‌کند این رکورد متعلق به کدام واحد است (برای اسکوپ‌های Unit و UnitAndBelow)
        Guid? OwnerOrganizationUnitId { get;    }

        // مشخص می‌کند این رکورد متعلق به کدام پست سازمانی است (برای اسکوپ Self)
        Guid? OwnerPositionId { get;  }

        // مشخص می‌کند این رکورد متعلق به کدام شخص است (برای اسکوپ Self)
        Guid? OwnerPersonId { get;  }

        Guid? OwnerUserId { get;  }
        void SetOwners(Guid? userId, Guid? personId, Guid? positiontId, Guid? orgUnitId);
        void SetPersonOwner(Guid personId);
        void SetUserOwner(Guid userId);
        void SetPositionOwner(Guid positiontId);
        void SetOrganizationUnitOwner(Guid orgUnitId);

        /* impelement
        public Guid? OwnerOrganizationUnitId { get; protected set; }
        public Guid? OwnerPositionId { get; protected set; }
        public Guid? OwnerPersonId { get; protected set; }
        public Guid? OwnerUserId { get; protected set; }

        public void SetOwners(Guid? userId, Guid? personId, Guid? positiontId, Guid? orgUnitId)
        {
            OwnerUserId = userId;
            OwnerPersonId = personId;
            OwnerPositionId = positiontId;
            OwnerOrganizationUnitId = orgUnitId;
        }
        public void SetPersonOwner(Guid personId)
        {
            OwnerPersonId = personId;
        }
        public void SetUserOwner(Guid userId)
        {
            OwnerUserId = userId;
        }
        public void SetPositionOwner(Guid positiontId)
        {
            OwnerPositionId = positiontId;
        }
        public void SetOrganizationUnitOwner(Guid orgUnitId)
        {
            OwnerOrganizationUnitId = orgUnitId;
        }
        
         */

    }
    public static class OwnerableEntityExtention
    {
        /*
        public static void SetOwners(this IOwnerableEntity entity,Guid? userId, Guid? personId, Guid? positiontId, Guid? orgUnitId)
        {
            entity.OwnerUserId = userId;
            entity.OwnerPersonId = personId;
            entity.OwnerPositionId = positiontId;
            entity.OwnerOrganizationUnitId = orgUnitId;
        }
        public static void SetPersonOwner(this IOwnerableEntity entity, Guid personId)
        {
            entity.OwnerPersonId = personId;
        }
        public static void SetUserOwner(this IOwnerableEntity entity, Guid userId)
        {
            entity.OwnerUserId = userId;
        }
        public static void SetPositionOwner(this IOwnerableEntity entity, Guid positiontId)
        {
            entity.OwnerPositionId = positiontId;
        }
        public static void SetOrganizationUnitOwner(this IOwnerableEntity entity, Guid orgUnitId)
        {
            entity.OwnerOrganizationUnitId = orgUnitId;
        }
        */
    }
}
