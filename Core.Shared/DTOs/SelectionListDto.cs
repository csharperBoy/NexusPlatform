using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.DTOs
{
    public class SelectionListDto
    {
        public SelectionListDto(string value, string display)
        {
            Value = value;
            Display = display;
        }
        public SelectionListDto(string value,string lable, string display)
        {
            Value = value;
            Label = lable;
            Display = display;
        }
        public string Value { get;private set; }
        public string Label { get;private set; }
        public string Display { get;private set; }
    }
}
