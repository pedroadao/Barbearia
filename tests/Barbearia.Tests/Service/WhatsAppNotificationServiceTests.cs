using System.Net;
using Barbearia.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Barbearia.Tests.Service;

public sealed class WhatsAppNotificationServiceTests
{
    private sealed class FakeHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public HttpRequestMessage? RequisicaoCapturada { get; private set; }

        public string? CorpoCapturado { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequisicaoCapturada = request;
            CorpoCapturado = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(statusCode);
        }
    }

    private static WhatsAppNotificationService CriarService(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.facebook.com/v21.0/"),
        };

        var options = Options.Create(new WhatsAppOptions
        {
            BaseUrl = "https://graph.facebook.com/v21.0/",
            PhoneNumberId = "123456",
            AccessToken = "token-de-teste",
        });

        return new WhatsAppNotificationService(httpClient, options, NullLogger<WhatsAppNotificationService>.Instance);
    }

    [Fact]
    public async Task EnviarAsync_DeveChamarUrlComVersaoDaApiPreservada()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var service = CriarService(handler);

        await service.EnviarAsync("+55 (21) 99277-8121", "mensagem de teste", CancellationToken.None);

        handler.RequisicaoCapturada.Should().NotBeNull();
        handler.RequisicaoCapturada!.RequestUri.Should().Be(
            new Uri("https://graph.facebook.com/v21.0/123456/messages"));
    }

    [Fact]
    public async Task EnviarAsync_DeveEnviarTelefoneApenasComDigitos()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var service = CriarService(handler);

        await service.EnviarAsync("+55 (21) 99277-8121", "mensagem de teste", CancellationToken.None);

        handler.CorpoCapturado.Should().Contain("\"to\":\"5521992778121\"");
    }

    [Fact]
    public async Task EnviarAsync_QuandoTelefoneVazio_NaoDeveFazerRequisicao()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var service = CriarService(handler);

        await service.EnviarAsync("   ", "mensagem de teste", CancellationToken.None);

        handler.RequisicaoCapturada.Should().BeNull();
    }
}
