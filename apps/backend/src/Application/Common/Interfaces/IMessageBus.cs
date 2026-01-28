namespace backend.Application.Common.Interfaces;

public interface IMessageBus
{
    /// <summary>
    /// Publishes a message to a specified exchange with a routing key.
    /// </summary>
    /// <typeparam name="T">The type of the message to publish.</typeparam>
    /// <param name="exchange">The name of the exchange to publish to.</param>
    /// <param name="routingKey">The routing key for the message.</param>
    /// <param name="message">The message object to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default) where T : class;
}
