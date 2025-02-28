using Application.Components.Messaging;
using Application.DbModel.Entities;

namespace Application.Messages
{
    public class TransactionValidatedEvent : IMessage
    {
        public Guid TransactionId { get; set; }
        public TransactionStatusEnum Status { get; set; }
    }

}
