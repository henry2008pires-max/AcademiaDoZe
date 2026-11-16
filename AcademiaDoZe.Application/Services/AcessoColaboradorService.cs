using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Exceptions;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoColaboradorService : IAcessoColaboradorService
{
    private readonly IAcessoColaboradorRepository _acessoRepository;
    private readonly IColaboradorRepository _colaboradorRepository;

    public AcessoColaboradorService(
        IAcessoColaboradorRepository acessoRepository,
        IColaboradorRepository colaboradorRepository)
    {
        _acessoRepository = acessoRepository;
        _colaboradorRepository = colaboradorRepository;
    }

    public async Task<AcessoColaboradorDTO> RegistrarAcesso(
        int colaboradorId,
        DateTime? dataHora = null,
        CancellationToken cancellationToken = default)
    {
        var colaborador = await _colaboradorRepository.ObterPorId(colaboradorId, cancellationToken)
            ?? throw new DomainException("COLABORADOR_NAO_ENCONTRADO");

        var result = AcessoColaborador.Criar(0, colaborador, dataHora ?? DateTime.Now);

        if (result.IsFailure)
            throw new DomainException(string.Join(", ", result.Notifications.Select(n => n.Mensagem)));

        var acesso = await _acessoRepository.Adicionar(result.Value!, cancellationToken);

        return acesso.ToDto();
    }

    public async Task<AcessoColaboradorDTO?> ObterPorId(int id, CancellationToken cancellationToken = default)
        => (await _acessoRepository.ObterPorId(id, cancellationToken))?.ToDto();

    public async Task<IEnumerable<AcessoColaboradorDTO>> ObterTodos(CancellationToken cancellationToken = default)
        => (await _acessoRepository.ObterTodos(cancellationToken)).Select(a => a.ToDto());

    public async Task<IEnumerable<AcessoColaboradorDTO>> ObterAcessosPorColaboradorPeriodo(
        int? colaboradorId = null,
        DateOnly? inicio = null,
        DateOnly? fim = null,
        CancellationToken cancellationToken = default)
    {
        if (inicio.HasValue && fim.HasValue && inicio > fim)
            throw new DomainException("PERIODO_INVALIDO");

        var acessos = await _acessoRepository.ObterAcessosPorColaboradorPeriodo(colaboradorId, inicio, fim, cancellationToken);

        return acessos.Select(a => a.ToDto());
    }

    public async Task<AcessoColaboradorDTO?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default)
        => (await _acessoRepository.ObterUltimoAcesso(colaboradorId, cancellationToken))?.ToDto();

    public Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
        => _acessoRepository.ObterHorasTrabalhadasNoDia(colaboradorId, data, cancellationToken);

    public Task<bool> Remover(int id, CancellationToken cancellationToken = default)
        => _acessoRepository.Remover(id, cancellationToken);
}
