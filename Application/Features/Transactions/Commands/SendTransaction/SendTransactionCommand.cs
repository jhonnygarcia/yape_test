using Application._Base;
using Application.Components.Messaging;
using Application.DbModel;
using Application.DbModel.Entities;
using Application.Messages;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Features.Transactions.Commands.SendTransaction;

namespace Application.Features.Transactions.Commands.SendTransaction
{
    public class SendTransactionCommand : IRequest<TransactionCreatedDto>
    {
        public Guid SourceAccountId { get; set; }
        public Guid TargetAccountId { get; set; }
        public int TransferTypeId { get; set; }
        public decimal Value { get; set; }
    }
    public class SendTransactionCommandHandler : IRequestHandler<SendTransactionCommand, TransactionCreatedDto>
    {
        private readonly IMessagingService _messagingService;
        private readonly AppDbContext _context;
        private const decimal LIMIT_TRANSACTION_PER_DAY = 20000;
        public SendTransactionCommandHandler(IMessagingService messagingService, AppDbContext context)
        {
            _messagingService = messagingService;
            _context = context;
        }

        public async Task<TransactionCreatedDto> Handle(SendTransactionCommand request, CancellationToken cancellationToken)
        {
            var sourceAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == request.SourceAccountId);
            var targetAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == request.TargetAccountId);

            if (sourceAccount == null)
            {
                throw new BusinessException("Source account does not exist");
            }
            if (targetAccount == null)
            {
                throw new BusinessException("Target account does not exist");
            }

            if (sourceAccount.Id == targetAccount.Id)
            {
                throw new BusinessException("The source account and destination account cannot be the same.");
            }

            if (request.TransferTypeId != (int)TransferTypeEnum.Deposit &&
                request.TransferTypeId != (int)TransferTypeEnum.Withdrawal)
            {
                throw new BusinessException("Invalid transfer type. Only deposits (1) and withdrawals (2) are allowed.");
            }

            if (request.TransferTypeId == (int)TransferTypeEnum.Deposit &&
                targetAccount.Balance < request.Value)
            {
                throw new BusinessException("Insufficient funds in the target account to complete the deposit.");
            }

            if (request.TransferTypeId == (int)TransferTypeEnum.Withdrawal &&
                sourceAccount.Balance < request.Value)
            {
                throw new BusinessException("Insufficient funds in the source account to complete the withdrawal.");
            }

            var totalTransactions = await _context.Transactions.Where(x =>
                        x.SourceAccountId == sourceAccount.Id &&
                        x.Status == TransactionStatusEnum.Approved).SumAsync(x => x.Value);

            if (totalTransactions + request.Value > LIMIT_TRANSACTION_PER_DAY)
            {
                var allowed = totalTransactions + request.Value - LIMIT_TRANSACTION_PER_DAY;
                throw new BusinessException($"Transaction exceeds the daily limit. You can transfer up to {allowed}.");
            }

            var messageEvent = new TransactionCreatedEvent
            {
                TransactionId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                SourceAccountId = request.SourceAccountId,
                TargetAccountId = request.TargetAccountId,
                TransferTypeId = request.TransferTypeId,
                Value = request.Value
            };

            await _messagingService.PublishAsync(messageEvent);

            return new TransactionCreatedDto(messageEvent.TransactionId);
        }
    }
}
