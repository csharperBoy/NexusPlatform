using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.DTOs
{
    public class TreeSelectionListDto
    {
        public TreeSelectionListDto(string value, string display, string? parentValue = null)
        {
            Value = value;
            Display = display;
            Label = display;
            ParentValue = parentValue;
        }
        public TreeSelectionListDto(string value, string lable, string display, string? parentValue = null)
        {
            Value = value;
            Label = lable;
            Display = display;
            ParentValue = parentValue;
        }
        public string Value { get; private set; }
        public string? ParentValue { get; private set; }
        public string Label { get; private set; }
        public string Display { get; private set; }
    }
}
