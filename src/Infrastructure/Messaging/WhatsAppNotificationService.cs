using System.Net.Http.Json;
using Barbearia.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Barbearia.Infrastructure.Messaging;

public sealed class WhatsAppNotificationService(
    HttpClient httpClient,
    IOptions<WhatsAppOptions> options,
    ILogger<WhatsAppNotificationService> logger) : IWhatsAppNotificationService
{
    public async Task EnviarAsync(string telefoneDestino, string mensagem, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(telefoneDestino))
        {
            logger.LogWarning("Envio de WhatsApp ignorado: telefone de destino não informado.");
            return;
        }

        try
        {
            var phoneNumberId = options.Value.PhoneNumberId;

            var payload = new
            {
                messaging_product = "whatsapp",
                to = telefoneDestino,
                type = "text",
                text = new { body = mensagem },
            };

            using var resposta = await httpClient.PostAsJsonAsync(
                $"/{phoneNumberId}/messages",
                payload,
                cancellationToken);

            if (!resposta.IsSuccessStatusCode)
            {
                var detalhe = await resposta.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError(
                    "Falha ao enviar mensagem via WhatsApp. StatusCode: {StatusCode}. Detalhe: {Detalhe}",
                    resposta.StatusCode,
                    detalhe);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao enviar mensagem via WhatsApp. A operação de negócio não foi afetada.");
        }
    }
}
