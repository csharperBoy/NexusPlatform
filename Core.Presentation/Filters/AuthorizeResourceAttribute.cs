using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Presentation.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AuthorizeResourceAttribute : TypeFilterAttribute
    {
        public AuthorizeResourceAttribute(string resourceKey, string action = "View")
            : base(typeof(AuthorizeResourceFilter))
        {
            Arguments = new object[] { resourceKey, action };
        }
    }
}
