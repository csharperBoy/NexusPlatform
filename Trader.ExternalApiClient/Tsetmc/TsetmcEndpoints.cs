namespace Trader.ExternalApiClient.Tsetmc;

internal static class TsetmcEndpoints
{
    /// <summary>GET /api/StaticData/GetStaticData</summary>
    public const string StaticData = "/api/StaticData/GetStaticData";

    /// <summary>GET /api/ClosingPrice/GetClosingPriceHistory/{insCode}/{dEven}</summary>
    public const string ClosingPriceHistory = "/api/ClosingPrice/GetClosingPriceHistory/{0}/{1}";

    /// <summary>GET /api/BestLimits/{insCode}/{dEven}</summary>
    public const string BestLimits = "/api/BestLimits/{0}/{1}";

    /// <summary>GET /api/Trade/GetTradeHistory/{insCode}/{dEven}/{withDetails}</summary>
    public const string TradeHistory = "/api/Trade/GetTradeHistory/{0}/{1}/{2}";

    /// <summary>GET /api/Instrument/GetInstrumentHistory/{insCode}/{dEven}</summary>
    public const string InstrumentHistory = "/api/Instrument/GetInstrumentHistory/{0}/{1}";

    /// <summary>GET /api/ClientType/GetClientTypeHistory/{insCode}/{dEven}</summary>
    public const string ClientTypeHistory = "/api/ClientType/GetClientTypeHistory/{0}/{1}";

    /// <summary>GET /api/ClosingPrice/GetClosingPriceDaily/{insCode}/{dEven}</summary>
    public const string ClosingPriceDaily = "/api/ClosingPrice/GetClosingPriceDaily/{0}/{1}";

    /// <summary>GET /api/MarketData/GetStaticThreshold/{insCode}/{dEven}</summary>
    public const string StaticThreshold = "/api/MarketData/GetStaticThreshold/{0}/{1}";

    /// <summary>GET /api/Shareholder/{insCode}/{dEven}</summary>
    public const string Shareholder = "/api/Shareholder/{0}/{1}";
}