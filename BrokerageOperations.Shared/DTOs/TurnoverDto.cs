using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerageOperations.Shared.DTOs
{
    public class TurnoverDto
    {
        public DateTime DateTime { get; set; }
        public string Description { get; set; }
        public long Deb { get; set; }
        public long Credit { get; set; }
    }
}
