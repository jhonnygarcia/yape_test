namespace Application.Features.Transactions.Commands.SendTransaction
{
    public class TransactionCreatedDto
    {
        public Guid TransactionId { get; set; }

        public TransactionCreatedDto(Guid transactionId)
        {
            TransactionId = transactionId;
        }
    }
}
