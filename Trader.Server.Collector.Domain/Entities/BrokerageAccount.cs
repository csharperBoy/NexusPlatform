using BrokerageOperations.Domain.Enums;
using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Server.Collector.Domain.Enums;

namespace Trader.Server.Collector.Domain.Entities
{
    public class BrokerageAccount : DataScopedAndResourcedEntity, IAggregateRoot
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public BrokeragePlatformEnum Platform { get; set; }
        public Guid FkUserId { get; set; }
    }
}
