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
        public ElementAccessPath(
                                 string _Title,
                                 string? _FullXpath = null,
                                 string? _Description = null,
                                 ElementPathEnum? _DefaultAccessPath = null,
                                 ElementTypeEnum? _ElementType = null,
                                 string? _Code = null,
                                 string? _xpath = null,
                                 string? _localXpathPart1 = null,
                                 string? _localXpathPart2 = null,
                                 string? _SelectorXpath = null,
                                 string? _JSpath = null,
                                 string _windowCode = "default",
                                 string _pageCode = "default",
                                 List<ElementAccessPath>? _children = null
                                )
        {
            Code = _Code;
            Title=_Title; FullXpath= _FullXpath;
            FullXpath = _FullXpath;
            Description = _Description;
            DefaultAccessPath = _DefaultAccessPath;
            Xpath = _xpath;
            localXpathPart1 = _localXpathPart1;
            localXpathPart2 = _localXpathPart2;
            SelectorXpath = _SelectorXpath;
            JSpath = _JSpath;
            windowCode = _windowCode;
            pageCode = _pageCode;
            Children = _children;
            ElementType = _ElementType;
        }
        public ElementPathEnum? DefaultAccessPath { get; set; } = null;
        public ElementTypeEnum? ElementType { get; set; } = null;

        public string Title { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string FullXpath { get; set; }
        public string Xpath { get; set; }
        public string localXpathPart1 { get; set; }
        public string localXpathPart2 { get; set; }

        public string SelectorXpath { get; set; }
        public string JSpath { get; set; }

        public string windowCode { get; set; } = "default";

        public string pageCode { get; set; } = "default";
        public List<ElementAccessPath>? Children { get; set; } = null;


        public string GenerateMixedXpath(string parentXpath , string param)
        {
            return $"{parentXpath}{localXpathPart1}{param}{localXpathPart2}";
        }
    }
}
