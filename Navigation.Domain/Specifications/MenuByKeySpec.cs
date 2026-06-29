using Base.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.Domain.Specifications
{
    public class MenuByKeySpec : BaseSpecification<Menu>
    {
        public MenuByKeySpec(string key)
            : base(r => r.Key.Trim() == key.Trim())
        {

            // بارگذاری Parent و Children
            AddInclude(r => r.Parent);
            AddInclude(r => r.Children);
        }
    }
}
