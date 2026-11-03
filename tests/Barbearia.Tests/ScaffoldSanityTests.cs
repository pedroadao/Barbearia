using FluentAssertions;

namespace Barbearia.Tests;

public class ScaffoldSanityTests
{
    [Fact]
    public void Projeto_de_testes_deve_compilar_e_executar()
    {
        var resultado = 1 + 1;

        resultado.Should().Be(2);
    }
}
