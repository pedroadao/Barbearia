using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Barbearia.Infrastructure.Messaging;

public sealed class RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options)
    : IRabbitMqConnectionProvider, IAsyncDisposable
{
    private readonly SemaphoreSlim _semaforo = new(1, 1);
    private IConnection? _conexao;

    public async Task<IChannel> CriarCanalAsync(CancellationToken cancellationToken)
    {
        var conexao = await ObterConexaoAsync(cancellationToken);

        return await conexao.CreateChannelAsync(cancellationToken: cancellationToken);
    }

    private async Task<IConnection> ObterConexaoAsync(CancellationToken cancellationToken)
    {
        if (_conexao is { IsOpen: true })
            return _conexao;

        await _semaforo.WaitAsync(cancellationToken);
        try
        {
            if (_conexao is { IsOpen: true })
                return _conexao;

            var rabbitMqOptions = options.Value;
            var factory = new ConnectionFactory
            {
                HostName = rabbitMqOptions.HostName,
                Port = rabbitMqOptions.Port,
                UserName = rabbitMqOptions.UserName,
                Password = rabbitMqOptions.Password,
            };

            _conexao = await factory.CreateConnectionAsync(cancellationToken);

            return _conexao;
        }
        finally
        {
            _semaforo.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_conexao is not null)
            await _conexao.DisposeAsync();

        _semaforo.Dispose();
    }
}
