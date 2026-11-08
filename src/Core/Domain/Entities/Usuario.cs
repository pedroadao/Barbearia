using Barbearia.Domain.Common;

namespace Barbearia.Domain.Entities;

public class Usuario
{
    protected Usuario()
    {
    }

    public Usuario(
        string nome,
        string email,
        string telefone,
        string? instagram,
        DateOnly dataNascimento,
        int idTipoUsuario)
    {
        Atualizar(nome, email, telefone, instagram, dataNascimento, idTipoUsuario);
    }

    public int Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Telefone { get; private set; } = string.Empty;

    public string? Instagram { get; private set; }

    public DateOnly DataNascimento { get; private set; }

    public int IdTipoUsuario { get; private set; }

    public TipoUsuario? TipoUsuario { get; private set; }
    public string? AuthUserId { get; private set; }
    public int IdStatus { get; private set; } = StatusIds.Ativo;
    public Status? Status { get; private set; }

    public void DefinirId(int id)
    {
        Id = id;
    }

    public void AssociarAuthUser(string? authUserId)
    {
        AuthUserId = authUserId;
    }

    public void MarcarComoExcluido()
    {
        IdStatus = StatusIds.Excluido;
    }

    public void Atualizar(
        string nome,
        string email,
        string telefone,
        string? instagram,
        DateOnly dataNascimento,
        int idTipoUsuario)
    {
        Nome = nome;
        Email = email;
        Telefone = telefone;
        Instagram = instagram;
        DataNascimento = dataNascimento;
        IdTipoUsuario = idTipoUsuario;
    }
}
