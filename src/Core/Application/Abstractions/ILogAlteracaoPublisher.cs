using Barbearia.Application.DTO;

namespace Barbearia.Application.Abstractions;

public interface ILogAlteracaoPublisher
{
    Task PublicarAsync(LogAlteracaoMensagem mensagem, CancellationToken cancellationToken);
}
