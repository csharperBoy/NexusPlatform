﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Models
{
    public class CorsSettings
    {
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    }
}