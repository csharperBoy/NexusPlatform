using Core.Domain.Specifications;
using Identity.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Specifications
{
    public class RefreshTokenByValueSpec : BaseSpecification<RefreshToken>
    {
        public RefreshTokenByValueSpec(string token)
            : base(r => r.Token == token)
        {
            // اگر نیاز به include داشتی اینجا اضافه کن
            // AddInclude(r => r.User);
        }
    }
}
