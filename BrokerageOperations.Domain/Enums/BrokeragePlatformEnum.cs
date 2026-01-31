using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerageOperations.Domain.Enums
{
    public enum BrokeragePlatformEnum
    {
        [Description("EasyTrader")] EasyTrader = 1,
        [Description("Agah")] Agah = 2
    }
    
}
