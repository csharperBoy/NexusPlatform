using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapper.Domain.Enums;

namespace WebScrapper.Application.DTOs
{
    public class TableElementAccessPath : ElementAccessPath
    {
        public TableElementAccessPath(string _Title, TableRowElementAccessPath _rowAccessPath, string? _FullXpath = null, string? _Description = null, ElementPathEnum? _DefaultAccessPath = null, ElementTypeEnum? _ElementType = null, string? _Code = null, string? _xpath = null, string? _localXpathPart1 = null, string? _localXpathPart2 = null, string? _SelectorXpath = null, string? _JSpath = null, string _windowCode = "default", string _pageCode = "default")
            : base(_Title, _FullXpath, _Description, _DefaultAccessPath, _ElementType, _Code, _xpath, _localXpathPart1, _localXpathPart2, _SelectorXpath, _JSpath, _windowCode, _pageCode)
        {
            rowAccessPath = _rowAccessPath;
        }
        public TableRowElementAccessPath rowAccessPath { get; set; }

        
        public TableRowElementAccessPath GetnumberOfRowFullXpath(string number)
        {
            try
            {
                return new TableRowElementAccessPath("ردیف اول جدول", rowAccessPath.columnsAccessPath,
                   $"{FullXpath}{rowAccessPath.localXpathPart1}{number}{rowAccessPath.localXpathPart2}",
                   rowAccessPath.Description,
                   rowAccessPath.DefaultAccessPath,
                   rowAccessPath.ElementType,
                   rowAccessPath.Code,
                   rowAccessPath.Xpath,
                   _windowCode: rowAccessPath.windowCode,
                   _pageCode: rowAccessPath.pageCode
                   );
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }

    public class TableRowElementAccessPath : ElementAccessPath
    {
        public TableRowElementAccessPath(string _Title, List<ElementAccessPath> _columnsAccessPath, string? _FullXpath = null, string? _Description = null, ElementPathEnum? _DefaultAccessPath = null, ElementTypeEnum? _ElementType = null, string? _Code = null, string? _xpath = null, string? _localXpathPart1 = null, string? _localXpathPart2 = null, string? _SelectorXpath = null, string? _JSpath = null, string _windowCode = "default", string _pageCode = "default")
            : base(_Title, _FullXpath, _Description, _DefaultAccessPath, _ElementType, _Code, _xpath, _localXpathPart1, _localXpathPart2, _SelectorXpath, _JSpath, _windowCode, _pageCode)
        {
            columnsAccessPath = _columnsAccessPath;
        }
        public List<ElementAccessPath> columnsAccessPath { get; set; }

    }

}
