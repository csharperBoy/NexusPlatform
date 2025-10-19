using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Models
{
    public class CacheSettings
    {
        public bool UseRedis { get; set; } = false;
        public int DefaultExpirationMinutes { get; set; } = 30;
        public string RedisInstanceName { get; set; } = "MyApp";
    }
}
