using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapper.Domain.Common;

namespace WebScrapper.Infrastructure.Common
{
    public class PlaywrightWindow : IWindowContract<PlaywrightPage>
    {
        public int  number { get; set; }
        public string code { get; set; }
        public string title { get; set; }
        public IEnumerable<PlaywrightPage> pages { get; set; }
    }
}
