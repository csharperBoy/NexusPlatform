using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.DTOs
{
    public class LocationInfoDto
    {
        public Guid Id { get; set; }

        public Guid ProfileId { get; set; }
        public string Title { get; set; }
    }
}
