using BrokerageOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerageOperations.Application.Interface
{
    public interface IBrokerageFactory
    {
        IBrokerageOperationsService CreateBrokerageService(BrokeragePlatformEnum platform);
    }
}
