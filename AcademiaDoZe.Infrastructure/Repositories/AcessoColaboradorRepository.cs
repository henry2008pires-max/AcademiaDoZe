// Luan Coelho

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
{
    // tb_acesso é compartilhada: pessoa_tipo 0 = Aluno, 1 = Colaborador
    private const int PessoaTipoColaborador = 1;

    public AcessoColaboradorRepository(
        string connectionString,
        DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    // já filtra pessoa_tipo = 1, então qualquer filtro adicional deve começar com AND
    private static string BaseSelectQuery => """
        SELECT
        ac.id_acesso, ac.pessoa_id, ac.data_hora,
        c.id_colaborador, c.cpf, c.nome AS colaborador_nome, c.nascimento, c.telefone, c.email, c.logradouro_id, c.numero, c.complemento, c.senha, c.foto, c.admissao, c.tipo, c.vinculo,
        l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
        FROM tb_acesso ac
        INNER JOIN tb_colaborador c ON ac.pessoa_id = c.id_colaborador
        INNER JOIN tb_logradouro l ON c.logradouro_id = l.id_logradouro
        WHERE ac.pessoa_tipo = 1
        """;

    public static AcessoColaborador Map(DbDataReader reader)
    {
        try
        {
            int id = reader.GetInt32Value("id_acesso");
            DateTime dataHora = reader.GetDateTimeValue("data_hora");

            var colaborador = ColaboradorRepository.Map(reader, "colaborador_nome");

            var result = AcessoColaborador.Criar(id, colaborador, dataHora);

            if (result.IsFailure)
            {
                throw new InfrastructureException(
                    "ERRO_DOMINIO_MAPEAMENTO",
                    $"Erro de domínio ao mapear acesso do colaborador ID {id}: {string.Join(", ",
                    result.Notifications.Select(n => n.Mensagem))}");
            }

            return result.Value!;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_ACESSO_COLABORADOR",
                $"Erro ao mapear dados do acesso do colaborador: {ex.Message}",
                ex);
        }
    }

    public async Task<AcessoColaborador?> ObterPorId(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND ac.id_acesso = @Id";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@Id", id, DbType.Int32);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            return await reader.ReadAsync(cancellationToken)
                ? Map(reader)
                : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_POR_ID",
                $"Erro ao obter acesso de colaborador por ID {id}: {ex.Message}",
                ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(
        CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY ac.data_hora DESC";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var acessos = new List<AcessoColaborador>();

            while (await reader.ReadAsync(cancellationToken))
            {
                acessos.Add(Map(reader));
            }

            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_TODOS",
                $"Erro ao obter todos os acessos de colaboradores: {ex.Message}",
                ex);
        }
    }

    public async Task<AcessoColaborador> Adicionar(
        AcessoColaborador entity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string query = FormatInsertQuery(
                "INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (@PessoaTipo, @PessoaId, @DataHora)");

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@PessoaTipo", PessoaTipoColaborador, DbType.Int32);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);

            int id = await command.ExecuteScalarIdAsync(
                "ERRO_ADICIONAR_ACESSO_COLABORADOR",
                "Falha ao obter ID inserido para o acesso do colaborador.",
                cancellationToken);

            var idProperty = typeof(Entity).GetProperty("Id");

            idProperty?.SetValue(entity, id);

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_ADICIONAR_ACESSO_COLABORADOR",
                $"Erro ao adicionar acesso para o colaborador ID {entity.ColaboradorId}: {ex.Message}",
                ex);
        }
    }

    public async Task<AcessoColaborador> Atualizar(
        AcessoColaborador entity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string query =
                "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaTipo", PessoaTipoColaborador, DbType.Int32);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);

            int rowsAffected =
                await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                throw new InfrastructureException(
                    "REGISTRO_NAO_ENCONTRADO",
                    $"Nenhum acesso de colaborador encontrado com o ID {entity.Id} para atualização.");
            }

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_ATUALIZAR_ACESSO_COLABORADOR",
                $"Erro ao atualizar acesso ID {entity.Id} do colaborador ID {entity.ColaboradorId}: {ex.Message}",
                ex);
        }
    }

    public async Task<bool> Remover(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@Id", id, DbType.Int32);
            command.AddParameter("@PessoaTipo", PessoaTipoColaborador, DbType.Int32);

            var result =
                await command.ExecuteNonQueryAsync(cancellationToken);

            return result > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_REMOVER_ACESSO_COLABORADOR",
                $"Erro ao remover acesso de colaborador ID {id}: {ex.Message}",
                ex);
        }
    }

    // todos os parâmetros são opcionais: sem filtro retorna todos os acessos
    // 'fim' é inclusivo (considera o dia inteiro)
    public async Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(
        int? colaboradorId = null,
        DateOnly? inicio = null,
        DateOnly? fim = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (inicioDt, fimExclusivoDt) = AcessoCalculos.ConverterPeriodo(inicio, fim);

            var filtros = new List<string>();
            if (colaboradorId.HasValue) filtros.Add("ac.pessoa_id = @ColaboradorId");
            if (inicioDt.HasValue) filtros.Add("ac.data_hora >= @Inicio");
            if (fimExclusivoDt.HasValue) filtros.Add("ac.data_hora < @Fim");

            string where = filtros.Count > 0 ? $" AND {string.Join(" AND ", filtros)}" : "";
            string query = $"{BaseSelectQuery}{where} ORDER BY ac.data_hora DESC";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            if (colaboradorId.HasValue) command.AddParameter("@ColaboradorId", colaboradorId.Value, DbType.Int32);
            if (inicioDt.HasValue) command.AddParameter("@Inicio", inicioDt.Value, DbType.DateTime);
            if (fimExclusivoDt.HasValue) command.AddParameter("@Fim", fimExclusivoDt.Value, DbType.DateTime);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var acessos = new List<AcessoColaborador>();

            while (await reader.ReadAsync(cancellationToken))
            {
                acessos.Add(Map(reader));
            }

            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_ACESSOS_PERIODO",
                $"Erro ao obter acessos de colaboradores por período: {ex.Message}",
                ex);
        }
    }

    public async Task<AcessoColaborador?> ObterUltimoAcesso(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // lê apenas a primeira linha do resultado ordenado (portável entre SGBDs: TOP x LIMIT)
            string query = $"{BaseSelectQuery} AND ac.pessoa_id = @ColaboradorId ORDER BY ac.data_hora DESC, ac.id_acesso DESC";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            return await reader.ReadAsync(cancellationToken)
                ? Map(reader)
                : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_ULTIMO_ACESSO",
                $"Erro ao obter o último acesso do colaborador ID {colaboradorId}: {ex.Message}",
                ex);
        }
    }

    // soma os pares entrada/saída do dia; uma entrada sem saída correspondente é ignorada
    public async Task<TimeSpan> ObterHorasTrabalhadasNoDia(
        int colaboradorId,
        DateOnly data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "SELECT data_hora FROM tb_acesso WHERE pessoa_tipo = @PessoaTipo AND pessoa_id = @ColaboradorId AND data_hora >= @Inicio AND data_hora < @Fim ORDER BY data_hora";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@PessoaTipo", PessoaTipoColaborador, DbType.Int32);
            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
            command.AddParameter("@Inicio", data.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            command.AddParameter("@Fim", data.AddDays(1).ToDateTime(TimeOnly.MinValue), DbType.DateTime);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var acessos = new List<DateTime>();

            while (await reader.ReadAsync(cancellationToken))
            {
                acessos.Add(reader.GetDateTimeValue("data_hora"));
            }

            return AcessoCalculos.Somar(AcessoCalculos.CalcularPermanencias(acessos));
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_HORAS_TRABALHADAS",
                $"Erro ao obter horas trabalhadas do colaborador ID {colaboradorId} em {data:dd/MM/yyyy}: {ex.Message}",
                ex);
        }
    }
}
