// Henry Pires

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class EntityTests
{
    [Fact(DisplayName = "Entity: id negativo -> lança DomainException")]
    public void Deve_Lancar_DomainException_Quando_IdNegativo()
    {
        Assert.Throws<DomainException>(() => Logradouro.Criar(-1, "12345-678", "Rua", "Bairro", "Cidade", "SP", "Brasil"));
    }

    [Theory(DisplayName = "Entity: id zero ou positivo -> não lança exceção")]
    [InlineData(0)]
    [InlineData(5)]
    public void Nao_Deve_Lancar_Excecao_Quando_IdValido(int id)
    {
        var exception = Record.Exception(() => Logradouro.Criar(id, "12345-678", "Rua", "Bairro", "Cidade", "SP", "Brasil"));
        Assert.Null(exception);
    }
}