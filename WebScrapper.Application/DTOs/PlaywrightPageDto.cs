using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapper.Domain.Common;
using Microsoft.Playwright;
namespace WebScrapper.Application.DTOs
{
    public class PlaywrightPageDto : IPageContract
    {

        public string code { get; set; }
        public int Number { get;  set ; }
        public string Title { get; set ; }
        public IPage page { get; set ; }



    }
}
