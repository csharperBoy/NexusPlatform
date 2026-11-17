// Trader.Presentation/Controllers/StrategyTuningController.cs
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Tuning;

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
}