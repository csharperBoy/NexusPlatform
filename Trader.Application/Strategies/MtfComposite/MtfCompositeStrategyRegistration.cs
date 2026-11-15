using Trader.Application.Abstractions.Indicators;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Indicators.Atr;
using Trader.Application.Indicators.Ema;
using Trader.Application.Indicators.Macd;
using Trader.Application.Indicators.Rsi;

namespace Trader.Application.Strategies.MtfComposite;

public sealed class MtfCompositeStrategyRegistration
    : StrategyRegistration<MtfCompositeStrategyConfig>
{
    private readonly IIndicator<EmaConfig, decimal?> _ema;
    private readonly IIndicator<MacdConfig, MacdResult?> _macd;
    private readonly IIndicator<AtrConfig, decimal?> _atr;
    private readonly IIndicator<RsiConfig, decimal?> _rsi;

    public MtfCompositeStrategyRegistration(
        IIndicator<EmaConfig, decimal?> ema,
        IIndicator<MacdConfig, MacdResult?> macd,
        IIndicator<AtrConfig, decimal?> atr,
        IIndicator<RsiConfig, decimal?> rsi)
    {
        _ema = ema;
        _macd = macd;
        _atr = atr;
        _rsi = rsi;
    }

    public const string KeyName = "mtf-composite";
    public const string DisplayNameValue = "Multi-TimeFrame Composite";

    public override string Key => KeyName;
    public override string DisplayName => DisplayNameValue;

    protected override IStrategy CreateStrategy(MtfCompositeStrategyConfig config)
        => new MtfCompositeStrategy(_ema, _macd, _atr, _rsi, config);
}