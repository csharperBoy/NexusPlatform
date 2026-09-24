using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities
{
    
    public class ExecutionLog : BaseEntity
    {
        public Guid PlanId { get; private set; }
        public Guid? OrderId { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }
        public ExecutionLogLevel Level { get; private set; }
        public string Message { get; private set; } = default!;
        public int? Code { get; private set; }

        private ExecutionLog() { }

        public static ExecutionLog Create(
            Guid planId,
            ExecutionLogLevel level,
            string message,
            Guid? orderId = null,
            int? code = null)
        {
            return new ExecutionLog
            {
                PlanId = planId,
                OrderId = orderId,
                Timestamp = DateTimeOffset.UtcNow,
                Level = level,
                Message = message,
                Code = code,
            };
        }
    }
}
