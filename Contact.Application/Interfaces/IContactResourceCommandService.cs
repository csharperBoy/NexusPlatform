using Contact.Application.Commands.ContactResource;
using Contact.Domain.Enums;
using Core.Domain.Common;
using Core.Shared.Enums.Contact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Interfaces
{
    public interface IContactResourceCommandService
    {
        Task<Guid> CreateContactResourceAsync(CreateContactResourceCommand request);
        Task DeleteContactResourceAsync(DeleteContactResourceCommand request);
        Task SaveAsync();
        Task<bool> UpdateContactResourceAsync(UpdateContactResourceCommand request);
    }
}
