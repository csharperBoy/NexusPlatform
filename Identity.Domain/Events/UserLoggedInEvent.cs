using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Events
{
    public class UserLoggedInEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string Username { get; }
        public DateTime LoginTime { get; }
        public string IpAddress { get; }

        public UserLoggedInEvent(Guid userId, string username, string ipAddress)
        {
            UserId = userId;
            Username = username;
            IpAddress = ipAddress;
            LoginTime = DateTime.UtcNow;
        }

        public DateTime OccurredOn => LoginTime;
    }
}