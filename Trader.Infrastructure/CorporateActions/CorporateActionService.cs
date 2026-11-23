using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.RegularExpressions;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Commands;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Abstractions.CorporateActions;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.CorporateActions;

public sealed class CorporateActionService : ICorporateActionService
{
    // "سود تقسیم شده:&nbsp;512 ریال"
    private static readonly Regex DividendAmountRegex = new(
        @"سود\s+تقسیم\s+شده[:\s\u00A0]*([\d,]+)",
        RegexOptions.Compiled);

    // "درصد افزایش:&nbsp;25%"
    private static readonly Regex CapitalRatioRegex = new(
        @"درصد\s+افزایش[:\s\u00A0]*([\d\.]+)%",
        RegexOptions.Compiled);

    private static readonly Regex HtmlTagRegex = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex NbspRegex = new(@"\u00A0", RegexOptions.Compiled);

    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly IBrokerClientFactory _brokerFactory;
    private readonly IBrokerSessionProvider _sessionProvider;
    private readonly CorporateActionsOptions _options;
    private readonly ILogger<CorporateActionService> _logger;

    public CorporateActionService(
        ITraderUnitOfWork<TraderDbContext> uow,
        IBrokerClientFactory brokerFactory,
        IBrokerSessionProvider sessionProvider,
        IOptions<CorporateActionsOptions> options,
        ILogger<CorporateActionService> logger)
    {
        _uow = uow;
        _brokerFactory = brokerFactory;
        _sessionProvider = sessionProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> FetchAndSaveAsync(
        string isin, DateTime from, DateTime to, CancellationToken ct = default)
    {
        if (_options.DataAccountId is null)
        {
            _logger.LogWarning(
                "CorporateActions:DataAccountId not configured — CA fetch skipped for {Isin}. " +
                "Set Trader:CorporateActions:DataAccountId to an active EasyTrader account.",
                isin);
            return 0;
        }

        ExternalApiClient.ExternalSystemSession session;
        try
        {
            session = await _sessionProvider.GetSessionAsync(_options.DataAccountId.Value, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to get session for CA fetch ({Isin}, account={AccountId}).",
                isin, _options.DataAccountId.Value);
            throw;
        }

        var reader = _brokerFactory.Get<IBrokerCorporateActionReader>(BrokerType.EasyTrader);
        var marks = await reader.GetCorporateActionsAsync(session, isin, from, to, ct);

        if (marks.Count == 0)
        {
            _logger.LogDebug("No CA marks for {Isin} in [{From}..{To}].", isin, from, to);
            return 0;
        }

        // Dedupe: load existing, build key set
        var existing = await _uow.CorporateActions.GetAllAsync(q =>
            q.Where(c => c.Isin == isin));

        var existingKeys = new HashSet<string>(
            existing.Select(c => $"{c.ExDateUtc:O}|{(int)c.ActionType}|{c.SourceId}"),
            StringComparer.OrdinalIgnoreCase);

        int saved = 0;
        foreach (var mark in marks)
        {
            var action = TryParse(isin, mark);
            if (action is null) continue;

            var key = $"{action.ExDateUtc:O}|{(int)action.ActionType}|{action.SourceId}";
            if (existingKeys.Contains(key)) continue;

            await _uow.CorporateActions.AddAsync(action);
            existingKeys.Add(key);
            saved++;
        }

        if (saved > 0)
            _logger.LogInformation(
                "Saved {Saved} new CA(s) for {Isin} (total marks={Total}).",
                saved, isin, marks.Count);

        return saved;
    }

    /* ═══════════════════ Parsing ═══════════════════ */

    private CorporateAction? TryParse(string isin, CorporateActionMarkDto mark)
    {
        var type = mark.Label?.ToUpperInvariant() switch
        {
            "D" => CorporateActionType.Dividend,
            "C" => CorporateActionType.BonusShares,   // MVP: capital → bonus
            _ => (CorporateActionType?)null,
        };
        if (type is null) return null;

        decimal? amount = null;
        decimal? ratio = null;

        if (type == CorporateActionType.Dividend)
        {
            var m = DividendAmountRegex.Match(mark.RawText ?? "");
            if (m.Success)
            {
                var raw = m.Groups[1].Value.Replace(",", "").Replace("،", "");
                if (decimal.TryParse(raw, NumberStyles.Any,
                        CultureInfo.InvariantCulture, out var d) && d > 0m)
                {
                    amount = d;
                }
            }
            if (amount is null) return null;  // نتوانستیم parse کنیم — skip
        }
        else // BonusShares
        {
            var m = CapitalRatioRegex.Match(mark.RawText ?? "");
            if (m.Success)
            {
                if (decimal.TryParse(m.Groups[1].Value, NumberStyles.Any,
                        CultureInfo.InvariantCulture, out var d) && d > 0m)
                {
                    ratio = d / 100m;
                }
            }
            if (ratio is null) return null;
        }

        var description = CleanHtml(mark.RawText);

        return CorporateAction.Create(
            isin: isin,
            exDateUtc: mark.ExDateUtc,
            actionType: type.Value,
            amount: amount,
            ratio: ratio,
            subscriptionPrice: null,   // MVP: null → treated as Bonus
            description: description.Length > 500 ? description[..500] : description,
            rawText: mark.RawText,
            sourceId: mark.SourceId);
    }

    private static string CleanHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return "";
        var stripped = HtmlTagRegex.Replace(html, " ");
        stripped = NbspRegex.Replace(stripped, " ");
        stripped = WhitespaceRegex.Replace(stripped, " ");
        return stripped.Trim();
    }
}