// Henry Pires

using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class LogradouroTests
{
    [Theory(DisplayName = "Logradouro: nome vazio -> NOME_OBRIGATORIO")]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_NomeVazio(string nome)
    {
        var result = Logradouro.Criar(1, "12345-678", nome, "Bairro", "Cidade", "SP", "Brasil");
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "NOME_OBRIGATORIO");
    }

    [Theory(DisplayName = "Logradouro: normaliza estado removendo espaços e upper")]
    [InlineData(" s p ", "SP")]
    [InlineData(" sp ", "SP")]
    public void Deve_Normalizar_Estado_Quando_InputContemEspacos(string inputEstado, string expected)
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua", "Bairro", "Cidade", inputEstado, "Brasil");
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Estado);
    }

    [Fact(DisplayName = "Logradouro: bairro vazio -> BAIRRO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_BairroVazio()
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua", "", "Cidade", "SP", "Brasil");
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "BAIRRO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Logradouro: país obrigatório -> PAIS_OBRIGATORIO")]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_PaisVazio(string pais)
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua", "Bairro", "Cidade", "SP", pais);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "PAIS_OBRIGATORIO");
    }

    [Fact(DisplayName = "Logradouro: cep inválido propaga notificação")]
    public void Deve_Falhar_Criacao_Quando_CepInvalido()
    {
        var result = Logradouro.Criar(1, "123", "Rua", "Bairro", "Cidade", "SP", "Brasil");
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CEP_DIGITOS");
    }
}