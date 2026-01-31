using BrokerageOperations.Domain.Enums;
using Core.Domain.Attributes;
using Core.Domain.Common;
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
    public class BrokerageAccount : DataScopedAndResourcedEntity, IAggregateRoot
    {
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
