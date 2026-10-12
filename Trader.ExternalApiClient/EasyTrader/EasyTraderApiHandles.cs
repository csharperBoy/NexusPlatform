using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.ExternalApiClient.EasyTrader
{
    /// <summary>
    /// Session مخصوص Login Flow.
    /// CookieContainer و تنظیمات مخصوص لاگین رو کپسوله می‌کنه تا Broker ندونه چیه.
    /// </summary>
    public sealed class LoginHttpSession : IDisposable
    {
        internal HttpClient Client { get; }
        internal LoginHttpSession(HttpClient client) => Client = client;
        public void Dispose() => Client.Dispose();
    }

    


}
