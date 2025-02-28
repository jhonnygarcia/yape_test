using Application.DbModel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Queries.GetAll
{
    public class GetAccountsQuery : IRequest<IEnumerable<AccountDto>>;
    public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, IEnumerable<AccountDto>>
    {
        private readonly AppDbContext _context;
        public GetAccountsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AccountDto>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Accounts.Select(x => new AccountDto
            {
                Id = x.Id,
                Balance = x.Balance,
                Name = x.Name
            }).ToListAsync(cancellationToken);
        }
    }
}
