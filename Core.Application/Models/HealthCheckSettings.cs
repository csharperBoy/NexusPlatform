using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Models
{
    public class HealthCheckSettings
    {
        public bool Enabled { get; set; } = true;
        public string Endpoint { get; set; } = "/health";
        public int TimeoutSeconds { get; set; } = 10;
    }
}