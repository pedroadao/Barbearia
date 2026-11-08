using System.Text;
using System.Text.Json;
using Barbearia.Application.DTO;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Barbearia.Infrastructure.Messaging;

public sealed class LogAlteracaoConsumer(
    IRabbitMqConnectionProvider connectionProvider,
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<LogAlteracaoConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = options.Value.QueueName;

        var canal = await connectionProvider.CriarCanalAsync(stoppingToken);

        await canal.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(canal);
        consumer.ReceivedAsync += async (_, evento) => await ProcessarMensagemAsync(canal, evento, stoppingToken);

        await canal.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Encerramento normal do serviço — nada a fazer.
        }
    }

    private async Task ProcessarMensagemAsync(
        IChannel canal,
        BasicDeliverEventArgs evento,
        CancellationToken cancellationToken)
    {
        try
        {
            var corpo = Encoding.UTF8.GetString(evento.Body.ToArray());
            var mensagem = JsonSerializer.Deserialize<LogAlteracaoMensagem>(corpo);

            if (mensagem is null)
            {
                await canal.BasicNackAsync(evento.DeliveryTag, multiple: false, requeue: false, cancellationToken);
                return;
            }

            using var scope = scopeFactory.CreateScope();
            var logAlteracaoDao = scope.ServiceProvider.GetRequiredService<ILogAlteracaoDao>();

            var logAlteracao = new LogAlteracao(
                mensagem.DataAlteracao,
                JsonSerializer.Serialize(mensagem.Dados),
                mensagem.IdUsuario,
                mensagem.Metodo,
                mensagem.Tabela);

            await logAlteracaoDao.AddAsync(logAlteracao, cancellationToken);

            await canal.BasicAckAsync(evento.DeliveryTag, multiple: false, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao consumir mensagem de log de alteração.");
            await canal.BasicNackAsync(evento.DeliveryTag, multiple: false, requeue: false, cancellationToken);
        }
    }
}
