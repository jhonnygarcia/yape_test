using Application.Components.Messaging;
using Application.DbModel;
using Application.DbModel.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Messages.Handlers
{
    public class TransactionCreateEventHandler : IMessageHandler<TransactionCreatedEvent>
    {
        private readonly IMessagingService _messagingService;
        private readonly ILogger<TransactionCreateEventHandler> _logger;
        private readonly AppDbContext _context;
        public TransactionCreateEventHandler(IMessagingService messagingService,
            ILogger<TransactionCreateEventHandler> logger, AppDbContext context)
        {
            _messagingService = messagingService;
            _logger = logger;
            _context = context;
        }

        public async Task HandleAsync(TransactionCreatedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Processing transaction {message.TransactionId} for validation.");

            var transaction = new Transaction
            {
                Id = message.TransactionId,
                CreatedAt = message.CreatedAt,
                SourceAccountId = message.SourceAccountId,
                TargetAccountId = message.TargetAccountId,
                Value = message.Value,
                Status = TransactionStatusEnum.Pending,
                TransferType = (TransferTypeEnum)message.TransferTypeId
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            var status = message.Value > 2000 ? TransactionStatusEnum.Rejected : TransactionStatusEnum.Approved;
            var validatedEvent = new TransactionValidatedEvent
            {
                TransactionId = message.TransactionId,
                Status = status
            };

            await _messagingService.PublishAsync(validatedEvent);
        }
    }
}
