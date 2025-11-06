using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Domain.Entities
{
    public class Sample
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string property1 { get; set; }
       
    }

}
