using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapper.Domain.Common
{
    public interface IWindowContract<TPage> where TPage : IPageContract
    {
        int number {  get;  }

        string code { get;  }
        string title { get; }
        IEnumerable<TPage> pages {  get; }
    }
}
