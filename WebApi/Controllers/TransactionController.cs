using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Transactions.Commands.SendTransaction;
using Application.Features.Transactions.Queries.GetFiltered;
using Application.Features.Accounts.Queries.GetAll;

namespace WebApi.Controllers;

[ApiController]
[Route(RoutePrefix)]
public class TransactionController : ControllerBase
{
    private const string RoutePrefix = "api/v1/transactions";
    private readonly IMediator _mediator;

    public TransactionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> Get([FromQuery] GetTransactionsFilteredQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionCreatedDto>> CreateTransaction([FromBody] SendTransactionCommand transaction)
    {
        var result = await _mediator.Send(transaction);
        return Ok(result);
    }
}
