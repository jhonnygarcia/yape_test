using Application.Components.Messaging;

namespace Application.Messages
{
    public class TransactionCreatedEvent : IMessage
    {
        public Guid TransactionId { get; set; }
        public Guid SourceAccountId { get; set; }
        public Guid TargetAccountId { get; set; }
        public int TransferTypeId { get; set; }
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
