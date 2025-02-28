namespace Application.Components.Messaging
{
    public interface IMessagingService
    {
        Task PublishAsync<T>(T message) where T : class, IMessage;
    }

}
