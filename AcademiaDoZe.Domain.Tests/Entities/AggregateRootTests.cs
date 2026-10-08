// Henry Pires

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AggregateRootTests
{
    private static Logradouro GetValidLogradouro() => Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;
    private static Arquivo GetValidArquivo() => Arquivo.Criar(new byte[] { 1, 2, 3 }).Value!;

    [Fact(DisplayName = "Logradouro deve ser um Aggregate Root")]
    public void Logradouro_Deve_Ser_AggregateRoot()
    {
        Assert.IsAssignableFrom<IAggregateRoot>(GetValidLogradouro());
    }

    [Fact(DisplayName = "Aluno deve ser um Aggregate Root")]
    public void Aluno_Deve_Ser_AggregateRoot()
    {
        var aluno = Aluno.Criar(1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo()).Value!;
        Assert.IsAssignableFrom<IAggregateRoot>(aluno);
    }

    [Fact(DisplayName = "Colaborador deve ser um Aggregate Root")]
    public void Colaborador_Deve_Ser_AggregateRoot()
    {
        var colaborador = Colaborador.Criar(1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo(),
            DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), ColaboradorTipo.Atendente, ColaboradorVinculo.CLT).Value!;
        Assert.IsAssignableFrom<IAggregateRoot>(colaborador);
    }

    [Fact(DisplayName = "Matricula deve ser um Aggregate Root")]
    public void Matricula_Deve_Ser_AggregateRoot()
    {
        var aluno = Aluno.Criar(1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo()).Value!;
        var matricula = Matricula.Criar(1, aluno, MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today), "Objetivo", MatriculaRestricoes.None, null).Value!;
        Assert.IsAssignableFrom<IAggregateRoot>(matricula);
    }

    [Fact(DisplayName = "AcessoAluno deve ser um Aggregate Root")]
    public void AcessoAluno_Deve_Ser_AggregateRoot()
    {
        var aluno = Aluno.Criar(1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo()).Value!;
        var acesso = AcessoAluno.Criar(1, aluno, DateTime.Today.AddHours(10)).Value!;
        Assert.IsAssignableFrom<IAggregateRoot>(acesso);
    }

    [Fact(DisplayName = "AcessoColaborador deve ser um Aggregate Root")]
    public void AcessoColaborador_Deve_Ser_AggregateRoot()
    {
        var colaborador = Colaborador.Criar(1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo(),
            DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), ColaboradorTipo.Atendente, ColaboradorVinculo.CLT).Value!;
        var acesso = AcessoColaborador.Criar(1, colaborador, DateTime.Today.AddHours(10)).Value!;
        Assert.IsAssignableFrom<IAggregateRoot>(acesso);
    }
}