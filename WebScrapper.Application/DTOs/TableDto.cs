using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapper.Application.DTOs
{
    public class TableDto
    {
        public List<TableRowDto> rows { get; set; }
    }
    public class TableRowDto
    {
        public List<TableColumnDto> columns { get; set; }
    }
    public class TableColumnDto
    {
        public string key { get; set; }
        public string value { get; set; }
    }

}
