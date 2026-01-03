using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Security
{
    public class ResourceDefinition
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // تبدیل به string
        public string Category { get; set; } // تبدیل به string
        public string ParentKey { get; set; } // استفاده از Key به جای Id
        public int DisplayOrder { get; set; }
        public string Icon { get; set; }
        public string Route { get; set; }
        public string[] Permissions { get; set; }
        public string[] RequiredFeatures { get; set; }



    }
}
