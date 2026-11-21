// Trader.Presentation/Controllers/StrategyTuningController.cs
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Tuning;
using Trader.Application.Strategies.Tuning;
using Trader.Domain.Enums;

namespace Trader.Presentation.Controllers;

[ApiController]
[Route("api/Trader/StrategyTuning")]
[Authorize]
[AuthorizeResource("trader.strategy")]
public sealed class StrategyTuningController : ControllerBase
{
    private readonly IStrategyTuningCommandService _cmd;
    private readonly IStrategyTuningQueryService _qry;

    public StrategyTuningController(
        IStrategyTuningCommandService cmd,
        IStrategyTuningQueryService qry)
    {
        _cmd = cmd; _qry = qry;
    }

    [HttpPost("Run")]
    public async Task<ActionResult<Guid>> Run(
        [FromBody] CreateStrategyTuningModel model, CancellationToken ct)
    {
        try
        {
            var id = await _cmd.CreateAsync(model, ct);
            await _cmd.SaveAsync(ct);
            return Ok(id);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("Cancel/{id:guid}")]
    public async Task<ActionResult<bool>> Cancel(Guid id, CancellationToken ct)
    {
        var ok = await _cmd.CancelAsync(id, ct);
        if (ok) await _cmd.SaveAsync(ct);
        return Ok(ok);
    }

    [HttpGet("GetList")]
    public async Task<ActionResult<IReadOnlyList<StrategyTuningRunSummaryView>>> GetList(
        [FromQuery] int take = 100, CancellationToken ct = default)
        => Ok(await _qry.GetListAsync(take, ct));

    [HttpGet("GetById/{id:guid}")]
    public async Task<ActionResult<StrategyTuningRunDetailView>> GetById(
        Guid id, CancellationToken ct)
    {
        var detail = await _qry.GetByIdAsync(id, ct);
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpGet("Results/{id:guid}")]
    public async Task<ActionResult<IReadOnlyList<StrategyTuningResultView>>> GetResults(
        Guid id, [FromQuery] bool topOnly = false, [FromQuery] int take = 100,
        CancellationToken ct = default)
        => Ok(await _qry.GetResultsAsync(id, topOnly, take, ct));

    // در StrategyTuningController.cs

    [HttpGet("ParameterSpace/{strategyKey}")]
    public ActionResult<IReadOnlyList<ParameterDefinitionView>> GetParameterSpace(
        string strategyKey, [FromQuery] int primaryTimeFrame = 1440)
    {
        var tuner = HttpContext.RequestServices
            .GetServices<IStrategyTuner>()
            .FirstOrDefault(t => t.TunerKey == strategyKey);
        if (tuner is null) return NotFound();

        var space = tuner.GetParameterSpace((TimeFrame)primaryTimeFrame);
        var views = space.Select(p => new ParameterDefinitionView(
            Name: p.Name,
            DisplayName: p.EffectiveDisplayName,
            TypeName: p switch
            {
                IntParameter => "int",
                DecimalParameter => "decimal",
                BoolParameter => "bool",
                TimeFrameParameter => "timeframe",
                _ => "unknown",
            },
            Importance: (int)p.Importance,
            ImportanceLabel: p.Importance.GetLabel(),
            Description: p.Description,
            MinValue: p switch
            {
                IntParameter i => i.Min,
                DecimalParameter d => d.Min,
                _ => null,
            },
            MaxValue: p switch
            {
                IntParameter i => i.Max,
                DecimalParameter d => d.Max,
                _ => null,
            },
            AllowedValues: p is TimeFrameParameter tfp
                ? tfp.AllowedValues.Cast<object>().ToList()
                : (p is BoolParameter ? new List<object> { true, false } : null)
        )).OrderByDescending(v => v.Importance).ToList();

        return Ok(views);
    }
}