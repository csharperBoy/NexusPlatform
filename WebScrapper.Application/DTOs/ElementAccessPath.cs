using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapper.Domain.Enums;

namespace WebScrapper.Application.DTOs
{
    public class ElementAccessPath
    {
        public ElementPathEnum? DefaultAccessPath { get; set; } = null;
        public string Title { get; set; }
        public string Description { get; set; }
        public string FullXpath { get; set; }
        public string localXpath { get; set; }
        public string SelectorXpath { get; set; }
        public string JSpath { get; set; }
    }
}
