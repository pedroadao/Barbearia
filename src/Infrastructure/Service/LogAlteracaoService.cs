using Barbearia.Application.DTO;
using Barbearia.Domain.Common;
using Barbearia.Infrastructure.Dao.Interface;
using Barbearia.Infrastructure.Service.Interface;

namespace Barbearia.Infrastructure.Service;

public sealed class LogAlteracaoService(ILogAlteracaoDao logAlteracaoDao, IUsuarioDao usuarioDao) : ILogAlteracaoService
{
    private const string NomeUsuarioDesconhecido = "Usuário removido";

    public async Task<Result<IReadOnlyList<LogAlteracaoDto>>> ObterComFiltroAsync(
        LogAlteracaoFiltroDto filtro,
        CancellationToken cancellationToken)
    {
        var logs = await logAlteracaoDao.ObterComFiltroAsync(
            filtro.DataInicio,
            filtro.DataFim,
            filtro.IdUsuario,
            cancellationToken);

        var idsUsuarios = logs.Select(l => l.IdUsuario).Distinct().ToList();
        var usuarios = await usuarioDao.GetByIdsAsync(idsUsuarios, cancellationToken);
        var nomesPorId = usuarios.ToDictionary(u => u.Id, u => u.Nome);

        return Result.Success<IReadOnlyList<LogAlteracaoDto>>(
            logs
                .Select(l => l.ToDto(nomesPorId.GetValueOrDefault(l.IdUsuario, NomeUsuarioDesconhecido)))
                .ToList());
    }
}
