// Henry Pires

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class ValueObjectsTests
{
    [Theory(DisplayName = "Cep: dígitos inválidos -> CEP_DIGITOS")]
    [InlineData("123")]
    [InlineData("12-345")]
    public void Deve_Falhar_Criacao_Quando_CepDigitosInvalidos(string input)
    {
        var result = Cep.Criar(input);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
    }

    [Theory(DisplayName = "Cep: formatos válidos (com e sem hífen)")]
    [InlineData("12345-678")]
    [InlineData("12345678")]
    public void Deve_Criar_Cep_Quando_Valido(string input)
    {
        var result = Cep.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal("12345678", result.Value!.Valor);
    }

    [Theory(DisplayName = "Cep: obrigatório -> CEP_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    public void Deve_Falhar_Criacao_Quando_CepNuloOuVazio(string? input)
    {
        var result = Cep.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CEP_OBRIGATORIO");
    }

    [Theory(DisplayName = "Cpf: obrigatório -> CPF_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_CpfNuloOuVazio(string? input)
    {
        var result = Cpf.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_OBRIGATORIO");
    }

    [Theory(DisplayName = "Cpf: formatos válidos (com e sem pontuação)")]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public void Deve_Criar_Cpf_Quando_Valido(string input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal("52998224725", result.Value!.Valor);
    }

    [Theory(DisplayName = "Cpf: sem quantidade de dígitos correta -> CPF_DIGITOS")]
    [InlineData("123")]
    [InlineData("abc")]
    public void Deve_Falhar_Criacao_Quando_CpfSemDigitosCorretos(string input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_DIGITOS");
    }

    [Theory(DisplayName = "Telefone: dígitos inválidos -> TELEFONE_DIGITOS")]
    [InlineData("1234")]
    [InlineData("(1)2345")]
    public void Deve_Falhar_Criacao_Quando_TelefoneDigitosInvalidos(string input)
    {
        var result = Telefone.Criar(input);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TELEFONE_DIGITOS");
    }

    [Theory(DisplayName = "Telefone: formatos válidos (com e sem formatação)")]
    [InlineData("(11) 91234-5678")]
    [InlineData("11912345678")]
    public void Deve_Criar_Telefone_Quando_Valido(string input)
    {
        var result = Telefone.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal("11912345678", result.Value!.Valor);
    }

    [Theory(DisplayName = "Telefone: obrigatório -> TELEFONE_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    public void Deve_Falhar_Criacao_Quando_TelefoneNuloOuVazio(string? input)
    {
        var result = Telefone.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TELEFONE_OBRIGATORIO");
    }

    [Theory(DisplayName = "Email: formatos inválidos -> EMAIL_FORMATO")]
    [InlineData("semarroba.com")]
    [InlineData("a@")]
    [InlineData("@dominio.com")]
    [InlineData("a@dominio.")]
    public void Deve_Falhar_Criacao_Quando_EmailFormatoInvalido(string input)
    {
        var result = Email.Criar(input);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "EMAIL_FORMATO");
    }

    [Theory(DisplayName = "Email: formatos válidos")]
    [InlineData("user@example.com")]
    [InlineData("nome.sobrenome@dominio.com.br")]
    public void Deve_Criar_Email_Quando_Valido(string input)
    {
        var result = Email.Criar(input);
        Assert.True(result.IsSuccess);
    }

    [Theory(DisplayName = "Senha: valida requisito de uppercase e tamanho -> SENHA_FORMATO")]
    [InlineData("abcdef")]
    [InlineData("Ab1")]
    public void Deve_Falhar_Criacao_Quando_SenhaFormatoInvalido(string input)
    {
        var result = Senha.Criar(input);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "SENHA_FORMATO");
    }

    [Theory(DisplayName = "Senha: obrigatório -> SENHA_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    public void Deve_Falhar_Criacao_Quando_SenhaNulaOuVazia(string? input)
    {
        var result = Senha.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "SENHA_OBRIGATORIO");
    }

    [Theory(DisplayName = "Senha: criação válida")]
    [InlineData("Abcdef")]
    [InlineData("Senha123")]
    public void Deve_Criar_Senha_Quando_Valida(string input)
    {
        var result = Senha.Criar(input);
        Assert.True(result.IsSuccess);
    }

    [Theory(DisplayName = "Endereco: criação válida com número e complemento")]
    [InlineData("10", "Bloco A")]
    [InlineData("1", "")]
    public void Deve_Criar_Endereco_Quando_Valido(string numero, string complemento)
    {
        var logradouro = Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;
        var result = Endereco.Criar(logradouro, numero, complemento);

        Assert.True(result.IsSuccess);
        Assert.Equal(logradouro.Id, result.Value!.LogradouroId);
        Assert.Equal(numero, result.Value.Numero);
        Assert.Equal(complemento, result.Value.Complemento);
    }

    [Theory(DisplayName = "Endereco: valida obrigatoriedade do logradouro e número")]
    [InlineData(null, "1", "LOGRADOURO_OBRIGATORIO")]
    [InlineData("valid", "", "NUMERO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_EnderecoInvalido(string? logradouroCase, string numero, string expected)
    {
        Logradouro? logradouro = null;
        if (logradouroCase == "valid")
            logradouro = Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

        var result = Endereco.Criar(logradouro!, numero, "");
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == expected);
    }

    [Fact(DisplayName = "Arquivo: criação válida")]
    public void Deve_Criar_Arquivo_Quando_Valido()
    {
        var result = Arquivo.Criar(new byte[] { 1, 2, 3 });
        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "Arquivo: nulo -> ARQUIVO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_ArquivoNulo()
    {
        var result = Arquivo.Criar(null!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_OBRIGATORIO");
    }

    [Fact(DisplayName = "Arquivo: acima do tamanho máximo -> ARQUIVO_TIPO_TAMANHO")]
    public void Deve_Falhar_Criacao_Quando_ArquivoExcedeTamanho()
    {
        var conteudo = new byte[(15 * 1024 * 1024) + 1];
        var result = Arquivo.Criar(conteudo);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_TIPO_TAMANHO");
    }

    [Fact(DisplayName = "Arquivo: no limite exato do tamanho -> sucesso")]
    public void Deve_Criar_Arquivo_Quando_NoLimiteExato()
    {
        var conteudo = new byte[15 * 1024 * 1024];
        var result = Arquivo.Criar(conteudo);
        Assert.True(result.IsSuccess);
    }
}