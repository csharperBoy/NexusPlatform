using Contact.Domain.Enums;
using Core.Shared.Enums.Contact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.DTOs
{
    public class ContactResourceInfoDto
    {

        public Guid Id { get;  set; }
        public Guid? ParentId { get;  set; }
        public ContactTypeEnum ContactType { get;  set; }
        public string Value { get;  set; }
        public string? Label { get;  set; }

        public bool IsPrimary { get;  set; } 
        public int? SortOrder { get;  set; } 

        public ContactRelationTypeEnum? RelationType { get; set; }
    }
}
