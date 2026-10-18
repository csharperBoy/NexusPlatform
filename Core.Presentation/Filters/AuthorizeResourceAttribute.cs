using Microsoft.AspNetCore.Mvc;

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
