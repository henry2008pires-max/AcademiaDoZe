using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Exceptions;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoAlunoService : IAcessoAlunoService
{
    private readonly IAcessoAlunoRepository _acessoRepository;
    private readonly IAlunoRepository _alunoRepository;
    private readonly IMatriculaRepository _matriculaRepository;

    public AcessoAlunoService(
        IAcessoAlunoRepository acessoRepository,
        IAlunoRepository alunoRepository,
        IMatriculaRepository matriculaRepository)
    {
        _acessoRepository = acessoRepository;
        _alunoRepository = alunoRepository;
        _matriculaRepository = matriculaRepository;
    }

    public async Task<AcessoAlunoDTO> RegistrarAcesso(
        int alunoId,
        DateTime? dataHora = null,
        CancellationToken cancellationToken = default)
    {
        var aluno = await _alunoRepository.ObterPorId(alunoId, cancellationToken)
            ?? throw new DomainException("ALUNO_NAO_ENCONTRADO");

        if (!await _matriculaRepository.PossuiMatriculaAtiva(alunoId, cancellationToken))
            throw new DomainException("MATRICULA_ATIVA_NAO_ENCONTRADA");

        var result = AcessoAluno.Criar(0, aluno, dataHora ?? DateTime.Now);

        if (result.IsFailure)
            throw new DomainException(string.Join(", ", result.Notifications.Select(n => n.Mensagem)));

        var acesso = await _acessoRepository.Adicionar(result.Value!, cancellationToken);

        return acesso.ToDto();
    }

    public async Task<AcessoAlunoDTO?> ObterPorId(int id, CancellationToken cancellationToken = default)
        => (await _acessoRepository.ObterPorId(id, cancellationToken))?.ToDto();

    public async Task<IEnumerable<AcessoAlunoDTO>> ObterTodos(CancellationToken cancellationToken = default)
        => (await _acessoRepository.ObterTodos(cancellationToken)).Select(a => a.ToDto());

    public async Task<IEnumerable<AcessoAlunoDTO>> ObterAcessosPorAlunoPeriodo(
        int? alunoId = null,
        DateOnly? inicio = null,
        DateOnly? fim = null,
        CancellationToken cancellationToken = default)
    {
        if (inicio.HasValue && fim.HasValue && inicio > fim)
            throw new DomainException("PERIODO_INVALIDO");

        var acessos = await _acessoRepository.ObterAcessosPorAlunoPeriodo(alunoId, inicio, fim, cancellationToken);

        return acessos.Select(a => a.ToDto());
    }

    public async Task<AcessoAlunoDTO?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default)
        => (await _acessoRepository.ObterUltimoAcesso(alunoId, cancellationToken))?.ToDto();

    public Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default)
        => _acessoRepository.EstaNaAcademia(alunoId, cancellationToken);

    public Task<bool> Remover(int id, CancellationToken cancellationToken = default)
        => _acessoRepository.Remover(id, cancellationToken);

    public Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default)
    {
        ValidarMes(mes);
        return _acessoRepository.ObterHorarioMaisProcuradoPorMes(mes, cancellationToken);
    }

    public Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default)
    {
        ValidarMes(mes);
        return _acessoRepository.ObterPermanenciaMediaPorMes(mes, cancellationToken);
    }

    public async Task<IEnumerable<AlunoResumoDTO>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default)
    {
        if (dias < 0)
            throw new DomainException("DIAS_INVALIDO");

        var alunos = await _acessoRepository.ObterAlunosSemAcessoNosUltimosDias(dias, cancellationToken);

        return alunos.Select(a => a.ToResumoDto());
    }

    private static void ValidarMes(int mes)
    {
        if (mes is < 1 or > 12)
            throw new DomainException("MES_INVALIDO");
    }
}
