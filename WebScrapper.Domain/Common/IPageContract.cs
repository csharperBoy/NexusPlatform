using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapper.Domain.Common
{
    public interface IPageContract
    {

         string code { get;  }
        int Number { get;  }
         string Title { get;  }
    }
}
