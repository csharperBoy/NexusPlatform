using Core.Shared.Enums.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.DTOs.Navigation
{
    public class MenuDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Key { get; set; }
        public string? Description { get; set; }
        public string Path { get; set; }
        /// <summary>
        ///  به صورت "fa-solid:folder" یا "md-folder" ذخیره می‌شود.
        ///  مثال: "fa-solid:folder" (Font Awesome) یا "md-folder" (Material Design).
        /// </summary>
        //public Icon? Icon { get; set; }
        public string? Icon { get; set; }
        public int? Order { get; set; }

        public Guid? ParentId { get; set; }
        public string? ParentKey { get; set; }


        //public List<MenuDto> Children { get; set; } = new();

        public IReadOnlyList<MenuDto> Children { get; init; } = Array.Empty<MenuDto>();

    }
}
