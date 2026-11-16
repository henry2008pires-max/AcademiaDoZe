using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoColaboradorService
{
    // regras: colaborador existente e horário permitido pelo domínio (06:00 às 22:00)
    Task<AcessoColaboradorDTO> RegistrarAcesso(int colaboradorId, DateTime? dataHora = null, CancellationToken cancellationToken = default);
    Task<AcessoColaboradorDTO?> ObterPorId(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoColaboradorDTO>> ObterTodos(CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoColaboradorDTO>> ObterAcessosPorColaboradorPeriodo(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default);
    Task<AcessoColaboradorDTO?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default);
    Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default);
    Task<bool> Remover(int id, CancellationToken cancellationToken = default);
}
