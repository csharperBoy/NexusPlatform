using Core.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sample.Application.Interfaces;
using Sample.Domain.Entities;


namespace Sample.Infrastructure.Data
{
    public static class SampleSeedData
    {
        public static async Task SeedEntityAsync(SampleEntity userManager, IConfiguration config)
        {

        }
       
    }
}