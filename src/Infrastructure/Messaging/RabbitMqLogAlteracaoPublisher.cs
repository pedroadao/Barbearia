using System.Text;
using System.Text.Json;
using Barbearia.Application.Abstractions;
using Barbearia.Application.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Barbearia.Infrastructure.Messaging;

public sealed class RabbitMqLogAlteracaoPublisher(
    IRabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqLogAlteracaoPublisher> logger) : ILogAlteracaoPublisher
{
    public async Task PublicarAsync(LogAlteracaoMensagem mensagem, CancellationToken cancellationToken)
    {
        try
        {
            var queueName = options.Value.QueueName;

            var canal = await connectionProvider.CriarCanalAsync(cancellationToken);
            await using (canal)
            {
                await canal.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: cancellationToken);

                var corpo = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mensagem));

                await canal.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: queueName,
                    body: corpo,
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Falha ao publicar log de alteração para a tabela {Tabela}. A operação de negócio não foi afetada.",
                mensagem.Tabela);
        }
    }
}
