using Application.Features.Accounts.Queries.GetAll;

namespace Application.Features.Transactions.Queries.GetFiltered
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public string TransferType { get; set; }
        public decimal Value { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Note { get; set; }
        public virtual AccountDto SourceAccount { get; set; }
        public virtual AccountDto TargetAccount { get; set; }
    }
}
