using Application.DbModel;
using Application.Features.Transactions.Queries.GetFiltered;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Queries.GetAll
{
    public class GetTransactionsFilteredQuery : IRequest<IEnumerable<TransactionDto>>
    {
        public Guid? SourceAccountId { get; set; }
        public Guid? TargetAccountId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
    public class GetTransactionsFilteredQueryHandler : IRequestHandler<GetTransactionsFilteredQuery, IEnumerable<TransactionDto>>
    {
        private readonly AppDbContext _context;
        public GetTransactionsFilteredQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TransactionDto>> Handle(GetTransactionsFilteredQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Transactions.AsQueryable();
            if (request.SourceAccountId.HasValue)
            {
                query = query.Where(x => x.SourceAccountId == request.SourceAccountId);
            }
            if (request.TargetAccountId.HasValue)
            {
                query = query.Where(x => x.SourceAccountId == request.TargetAccountId);
            }
            if (request.DateFrom.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= request.DateFrom.Value);
            }
            if (request.DateTo.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= request.DateTo.Value);
            }

            var result = await query.OrderByDescending(x => x.CreatedAt).Select(x => new TransactionDto
            {
                Id = x.Id,
                SourceAccount = new AccountDto
                {
                    Id = x.SourceAccount.Id,
                    Name = x.SourceAccount.Name,
                    Balance = x.SourceAccount.Balance
                },
                TargetAccount = new AccountDto
                {
                    Id = x.TargetAccount.Id,
                    Name = x.TargetAccount.Name,
                    Balance = x.TargetAccount.Balance
                },
                CreatedAt = x.CreatedAt,
                Note = x.Note,
                Status = x.Status.ToString(),
                TransferType = x.TransferType.ToString(),
                Value = x.Value

            }).ToListAsync(cancellationToken);

            return result;
        }
    }
}
