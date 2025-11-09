using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Shared.Events
{
    public class UserRegisteredEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string Username { get; }
        public string Email { get; }
        public DateTime RegistrationTime { get; }

        public UserRegisteredEvent(Guid userId, string username, string email)
        {
            UserId = userId;
            Username = username;
            Email = email;
            RegistrationTime = DateTime.UtcNow;
        }

        public DateTime OccurredOn => RegistrationTime;
    }
}