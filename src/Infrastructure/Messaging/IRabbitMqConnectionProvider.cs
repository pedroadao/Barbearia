using RabbitMQ.Client;

namespace Barbearia.Infrastructure.Messaging;

public interface IRabbitMqConnectionProvider
{
    Task<IChannel> CriarCanalAsync(CancellationToken cancellationToken);
}
