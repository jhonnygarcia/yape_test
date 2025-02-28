namespace Application.Components.Messaging
{
    public interface IMessageHandler<T> where T : IMessage
    {
        Task HandleAsync(T message, CancellationToken cancellationToken);
    }
}
