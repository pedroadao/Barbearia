namespace Barbearia.Application.Abstractions;

public interface IWhatsAppNotificationService
{
    Task EnviarAsync(string telefoneDestino, string mensagem, CancellationToken cancellationToken);
}
