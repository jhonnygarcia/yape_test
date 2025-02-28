using Application.Features.Accounts.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route(RoutePrefix)]
public class AccountController : ControllerBase
{
    private const string RoutePrefix = "api/v1/accounts";
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> Get()
    {
        var result = await _mediator.Send(new GetAccountsQuery());
        return Ok(result);
    }
}
