using BrokerageOperations.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderServer.Application.DTO;

namespace TraderServer.Application.Interface
{
    public interface ICollectorService
    {
        Task SaveSnapShotStockTrading(SaveSnapShotRequest req);
    }
}
