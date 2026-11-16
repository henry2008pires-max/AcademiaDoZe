using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoAlunoService
{
    // regras: aluno existente, matrícula ativa e horário permitido pelo domínio (06:00 às 22:00)
    Task<AcessoAlunoDTO> RegistrarAcesso(int alunoId, DateTime? dataHora = null, CancellationToken cancellationToken = default);
    Task<AcessoAlunoDTO?> ObterPorId(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoAlunoDTO>> ObterTodos(CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoAlunoDTO>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default);
    Task<AcessoAlunoDTO?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default);
    Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default);
    Task<bool> Remover(int id, CancellationToken cancellationToken = default);
    Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default);
    Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default);
    Task<IEnumerable<AlunoResumoDTO>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default);
}
