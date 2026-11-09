using System.Net.Http.Json;
using System.Text.RegularExpressions;
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
        var telefoneApenasDigitos = Regex.Replace(telefoneDestino ?? string.Empty, "[^0-9]", "");

        if (string.IsNullOrWhiteSpace(telefoneApenasDigitos))
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
                to = telefoneApenasDigitos,
                type = "text",
                text = new { body = mensagem },
            };

            using var resposta = await httpClient.PostAsJsonAsync(
                $"{phoneNumberId}/messages",
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
