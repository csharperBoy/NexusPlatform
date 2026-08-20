using Core.Application.Abstractions.Contact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Interfaces
{
    public interface IContactInternalService : IContactPublicService
    {
        Task ExpireAllContactAsync(Guid ProfileId);
        Task DeActiveContactProfileAsync(Guid ProfileId);
    }
}
