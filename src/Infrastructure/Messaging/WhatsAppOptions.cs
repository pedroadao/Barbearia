namespace Barbearia.Infrastructure.Messaging;

public sealed class WhatsAppOptions
{
    public const string SectionName = "WhatsApp";

    public string BaseUrl { get; set; } = "https://graph.facebook.com/v21.0";

    public string PhoneNumberId { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;
}
