using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapper.Application.DTOs
{
    public class MasterDetailTableDto
    {
        public TableRowDto MasterRow { get; set; } = new TableRowDto();
        public List<TableRowDto> DetailRows { get; set; } = new List<TableRowDto>();
    }
}
