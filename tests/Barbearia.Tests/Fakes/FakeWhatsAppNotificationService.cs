using Barbearia.Application.Abstractions;

namespace Barbearia.Tests.Fakes;

internal sealed class FakeWhatsAppNotificationService : IWhatsAppNotificationService
{
    public List<(string Telefone, string Mensagem)> MensagensEnviadas { get; } = [];

    public Task EnviarAsync(string telefoneDestino, string mensagem, CancellationToken cancellationToken)
    {
        MensagensEnviadas.Add((telefoneDestino, mensagem));
        return Task.CompletedTask;
    }
}
