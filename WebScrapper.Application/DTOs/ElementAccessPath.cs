using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapper.Application.DTOs
{
    public class ElementAccessPath
    {

        public string Title { get; set; }
        public string Description { get; set; }
        public string FullXpath { get; set; }
        public string localXpath { get; set; }
        public string SelectorXpath { get; set; }
        public string JSpath { get; set; }
    }
}
