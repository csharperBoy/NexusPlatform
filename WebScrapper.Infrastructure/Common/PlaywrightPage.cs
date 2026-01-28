using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapper.Domain.Common;
using static System.Net.Mime.MediaTypeNames;
namespace WebScrapper.Infrastructure.Common
{
    public class PlaywrightPage : IPageContract
    {

        public string code { get; set; }
        public int Number { get;  set ; }
        public string Title { get; set ; }
        public IPage page { get; set ; }



    }
}
