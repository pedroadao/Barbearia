using Barbearia.Domain.Common;

namespace Barbearia.Domain.Entities;

public sealed class Calendario
{
    private Calendario()
    {
    }

    public Calendario(
        int idBarbeiro,
        DateOnly data,
        TimeOnly horaInicio,
        TimeOnly horaFim,
        int? idCliente,
        bool disponivel,
        string? nomeClienteAvulso = null)
    {
        IdBarbeiro = idBarbeiro;
        Data = data;
        HoraInicio = horaInicio;
        HoraFim = horaFim;
        IdCliente = idCliente;
        Disponivel = disponivel;
        NomeClienteAvulso = nomeClienteAvulso;
    }

    public int Id { get; private set; }

    public int IdBarbeiro { get; private set; }

    public DateOnly Data { get; private set; }

    public TimeOnly HoraInicio { get; private set; }

    public TimeOnly HoraFim { get; private set; }

    public int? IdCliente { get; private set; }

    public bool Disponivel { get; private set; }

    public string? NomeClienteAvulso { get; private set; }

    public Usuario? Barbeiro { get; private set; }

    public Usuario? Cliente { get; private set; }

    public int IdStatus { get; private set; } = StatusIds.Ativo;

    public Status? Status { get; private set; }
    
    public void MarcarComoExcluido()
    {
        IdStatus = StatusIds.Excluido;
    }

    public void ReservarParaClienteAvulso(string nomeClienteAvulso)
    {
        IdCliente = null;
        NomeClienteAvulso = nomeClienteAvulso;
        Disponivel = false;
    }

    public void TransferirCliente(int? idCliente, string? nomeClienteAvulso)
    {
        IdCliente = idCliente;
        NomeClienteAvulso = nomeClienteAvulso;
    }
}
