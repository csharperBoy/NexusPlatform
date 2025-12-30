using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SecuredResourceAttribute : Attribute
    {
        public string ResourceKey { get; }
        public SecuredResourceAttribute(string resourceKey)
        {
            ResourceKey = resourceKey;
        }
    }
}
