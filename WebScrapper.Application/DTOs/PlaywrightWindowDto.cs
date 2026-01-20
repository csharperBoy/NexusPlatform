using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapper.Domain.Common;

namespace WebScrapper.Application.DTOs
{
    public class PlaywrightWindowDto : IWindowContract<PlaywrightPageDto>
    {
        public int  number { get; set; }
        public string code { get; set; }
        public string title { get; set; }
        public IEnumerable<PlaywrightPageDto> pages { get; set; }
    }
}
