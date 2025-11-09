using Core.Shared.Results;
using Sample.Application.DTOs;
using Sample.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Application.Interfaces
{

    /// <summary>
    /// اینترفیس سرویس اصلی ماژول Sample
    /// </summary>
    public interface ISampleService
    {
        /// <summary>
        /// ایجاد یک Sample جدید و تولید Domain Event
        /// </summary>
        Task<Result<SampleApiResponse>> SampleApiMethodAsync(SampleApiRequest request);

        /// <summary>
        /// ایجاد Sample جدید و پاک‌سازی کش مرتبط
        /// </summary>
        Task<Result<SampleApiResponse>> SampleApiMethodWithCacheAsync(SampleApiRequest request);


    }
}
