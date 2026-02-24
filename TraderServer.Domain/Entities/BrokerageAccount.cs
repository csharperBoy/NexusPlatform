using BrokerageOperations.Domain.Enums;
using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderServer.Domain.Enums;

namespace TraderServer.Domain.Entities
{

    [SecuredResource("Trader.BrokerageAccount")]
    public class BrokerageAccount : AuditableEntity,IDataScopedEntity, IAggregateRoot
    {
        #region IDataScopedEntity Impelement
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
        #endregion
        public BrokerageAccount()
        {
            
        }
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public BrokeragePlatformEnum Platform { get; set; }
        public Guid FkUserId { get; set; }
    }
}
