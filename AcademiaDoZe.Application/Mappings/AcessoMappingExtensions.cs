using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;

public static class AcessoMappingExtensions
{
    public static AcessoAlunoDTO ToDto(this AcessoAluno acesso)
        => new(acesso.Id, acesso.AlunoId, acesso.DataHora);

    public static AcessoColaboradorDTO ToDto(this AcessoColaborador acesso)
        => new(acesso.Id, acesso.ColaboradorId, acesso.DataHora);

    public static AlunoResumoDTO ToResumoDto(this Aluno aluno)
        => new(aluno.Id, aluno.Nome, aluno.Cpf.Valor, aluno.Email.Valor, aluno.Telefone.Valor);
}
