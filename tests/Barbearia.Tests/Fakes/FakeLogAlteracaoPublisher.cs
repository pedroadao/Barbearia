using Barbearia.Application.Abstractions;
using Barbearia.Application.DTO;

namespace Barbearia.Tests.Fakes;

internal sealed class FakeLogAlteracaoPublisher : ILogAlteracaoPublisher
{
    public List<LogAlteracaoMensagem> MensagensPublicadas { get; } = [];

    public Task PublicarAsync(LogAlteracaoMensagem mensagem, CancellationToken cancellationToken)
    {
        MensagensPublicadas.Add(mensagem);
        return Task.CompletedTask;
    }
}
