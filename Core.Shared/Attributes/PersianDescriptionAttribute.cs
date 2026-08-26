using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.Attributes
{
    public class PersianDescriptionAttribute : Attribute
    {
        public string Description { get; }

        public PersianDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
