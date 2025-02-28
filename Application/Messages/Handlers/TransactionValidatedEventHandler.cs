using Application.Components.Messaging;
using Application.DbModel;
using Application.DbModel.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Messages.Handlers
{
    public class TransactionValidatedEventHandler : IMessageHandler<TransactionValidatedEvent>
    {
        private readonly ILogger<TransactionValidatedEventHandler> _logger;
        private readonly AppDbContext _context;
        private const decimal LIMIT_TRANSACTION_PER_DAY = 20000;
        private const decimal LIMIT_PER_TRANSACTION = 2000;
        public TransactionValidatedEventHandler(ILogger<TransactionValidatedEventHandler> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task HandleAsync(TransactionValidatedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Validate transaction {message.TransactionId}");

            var transaction = await _context.Transactions
                .Include(x => x.SourceAccount)
                .Include(x => x.TargetAccount)
                .FirstOrDefaultAsync(x => x.Id == message.TransactionId);

            if (transaction == null)
            {
                _logger.LogWarning($"Notfound transaction {message.TransactionId}");
                return;
            }

            switch (message.Status)
            {
                case TransactionStatusEnum.Approved:
                    {
                        var totalTransactions = await _context.Transactions.Where(x =>
                            x.SourceAccountId == transaction.SourceAccountId &&
                            x.Status == TransactionStatusEnum.Approved).SumAsync(x => x.Value);

                        if (totalTransactions + transaction.Value > LIMIT_TRANSACTION_PER_DAY)
                        {
                            transaction.Status = TransactionStatusEnum.Rejected;
                            transaction.Note = $"Transaction exceeds the daily limit";
                            break;
                        }

                        if (transaction.TransferType == TransferTypeEnum.Deposit)
                        {
                            if (transaction.TargetAccount.Balance > transaction.Value)
                            {
                                transaction.Status = TransactionStatusEnum.Approved;
                                transaction.TargetAccount.Balance -= transaction.Value;
                                transaction.SourceAccount.Balance += transaction.Value;
                                transaction.Note = "Successfully completed";
                            }
                            else
                            {
                                transaction.Status = TransactionStatusEnum.Rejected;
                                transaction.Note = "Insufficient funds in the target account to complete the deposit.";
                            }
                        }

                        if (transaction.TransferType == TransferTypeEnum.Withdrawal)
                        {
                            if (transaction.SourceAccount.Balance > transaction.Value)
                            {
                                transaction.Status = TransactionStatusEnum.Approved;
                                transaction.SourceAccount.Balance -= transaction.Value;
                                transaction.TargetAccount.Balance += transaction.Value;
                                transaction.Note = "Successfully completed";
                            }
                            else
                            {
                                transaction.Status = TransactionStatusEnum.Rejected;
                                transaction.Note = "Insufficient funds in the source account to complete the withdrawal.";
                            }
                        }
                    }
                    break;
                case TransactionStatusEnum.Rejected:
                    {
                        transaction.Status = message.Status;
                        transaction.Note = transaction.Value > LIMIT_PER_TRANSACTION
                            ? $"The limit per transaction is {LIMIT_PER_TRANSACTION}"
                            : "Transaction rejected";

                        break;
                    }
                case TransactionStatusEnum.Pending:
                    break;
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
