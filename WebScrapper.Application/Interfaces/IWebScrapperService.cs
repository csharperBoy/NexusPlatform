using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapper.Application.DTOs;
namespace WebScrapper.Application.Interfaces
{
    

    public interface IWebScrapperService
    {
        Task<Result<SampleApiResponse>> SampleApiMethodAsync(ElementAccessPath request);

        Task<Result<SampleApiResponse>> SampleApiMethodWithCacheAsync(ElementAccessPath request);
    }
}
