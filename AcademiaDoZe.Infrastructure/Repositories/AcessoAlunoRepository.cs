// Luan Coelho

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    // tb_acesso é compartilhada: pessoa_tipo 0 = Aluno, 1 = Colaborador
    private const int PessoaTipoAluno = 0;

    public AcessoAlunoRepository(
        string connectionString,
        DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    // já filtra pessoa_tipo = 0, então qualquer filtro adicional deve começar com AND
    private static string BaseSelectQuery => """
        SELECT
        ac.id_acesso, ac.pessoa_id, ac.data_hora,
        a.id_aluno, a.cpf, a.nome AS aluno_nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
        l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
        FROM tb_acesso ac
        INNER JOIN tb_aluno a ON ac.pessoa_id = a.id_aluno
        INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro
        WHERE ac.pessoa_tipo = 0
        """;

    private static string AlunoSelectQuery => """
        SELECT
        a.id_aluno, a.cpf, a.nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
        l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
        FROM tb_aluno a
        INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro
        """;

    public static AcessoAluno Map(DbDataReader reader)
    {
        try
        {
            int id = reader.GetInt32Value("id_acesso");
            DateTime dataHora = reader.GetDateTimeValue("data_hora");

            var aluno = AlunoRepository.Map(reader, "aluno_nome");

            var result = AcessoAluno.Criar(id, aluno, dataHora);

            if (result.IsFailure)
            {
                throw new InfrastructureException(
                    "ERRO_DOMINIO_MAPEAMENTO",
                    $"Erro de domínio ao mapear acesso do aluno ID {id}: {string.Join(", ",
                    result.Notifications.Select(n => n.Mensagem))}");
            }

            return result.Value!;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_ACESSO_ALUNO",
                $"Erro ao mapear dados do acesso do aluno: {ex.Message}",
                ex);
        }
    }

    public async Task<AcessoAluno?> ObterPorId(
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
                $"Erro ao obter acesso de aluno por ID {id}: {ex.Message}",
                ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(
        CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY ac.data_hora DESC";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var acessos = new List<AcessoAluno>();

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
                $"Erro ao obter todos os acessos de alunos: {ex.Message}",
                ex);
        }
    }

    public async Task<AcessoAluno> Adicionar(
        AcessoAluno entity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string query = FormatInsertQuery(
                "INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (@PessoaTipo, @PessoaId, @DataHora)");

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);

            int id = await command.ExecuteScalarIdAsync(
                "ERRO_ADICIONAR_ACESSO_ALUNO",
                "Falha ao obter ID inserido para o acesso do aluno.",
                cancellationToken);

            var idProperty = typeof(Entity).GetProperty("Id");

            idProperty?.SetValue(entity, id);

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_ADICIONAR_ACESSO_ALUNO",
                $"Erro ao adicionar acesso para o aluno ID {entity.AlunoId}: {ex.Message}",
                ex);
        }
    }

    public async Task<AcessoAluno> Atualizar(
        AcessoAluno entity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string query =
                "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);

            int rowsAffected =
                await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                throw new InfrastructureException(
                    "REGISTRO_NAO_ENCONTRADO",
                    $"Nenhum acesso de aluno encontrado com o ID {entity.Id} para atualização.");
            }

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_ATUALIZAR_ACESSO_ALUNO",
                $"Erro ao atualizar acesso ID {entity.Id} do aluno ID {entity.AlunoId}: {ex.Message}",
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
            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);

            var result =
                await command.ExecuteNonQueryAsync(cancellationToken);

            return result > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_REMOVER_ACESSO_ALUNO",
                $"Erro ao remover acesso de aluno ID {id}: {ex.Message}",
                ex);
        }
    }

    // todos os parâmetros são opcionais: sem filtro retorna todos os acessos
    // 'fim' é inclusivo (considera o dia inteiro)
    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(
        int? alunoId = null,
        DateOnly? inicio = null,
        DateOnly? fim = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (inicioDt, fimExclusivoDt) = AcessoCalculos.ConverterPeriodo(inicio, fim);

            var filtros = new List<string>();
            if (alunoId.HasValue) filtros.Add("ac.pessoa_id = @AlunoId");
            if (inicioDt.HasValue) filtros.Add("ac.data_hora >= @Inicio");
            if (fimExclusivoDt.HasValue) filtros.Add("ac.data_hora < @Fim");

            string where = filtros.Count > 0 ? $" AND {string.Join(" AND ", filtros)}" : "";
            string query = $"{BaseSelectQuery}{where} ORDER BY ac.data_hora DESC";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            if (alunoId.HasValue) command.AddParameter("@AlunoId", alunoId.Value, DbType.Int32);
            if (inicioDt.HasValue) command.AddParameter("@Inicio", inicioDt.Value, DbType.DateTime);
            if (fimExclusivoDt.HasValue) command.AddParameter("@Fim", fimExclusivoDt.Value, DbType.DateTime);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var acessos = new List<AcessoAluno>();

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
                $"Erro ao obter acessos de alunos por período: {ex.Message}",
                ex);
        }
    }

    public async Task<AcessoAluno?> ObterUltimoAcesso(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // lê apenas a primeira linha do resultado ordenado (portável entre SGBDs: TOP x LIMIT)
            string query = $"{BaseSelectQuery} AND ac.pessoa_id = @AlunoId ORDER BY ac.data_hora DESC, ac.id_acesso DESC";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@AlunoId", alunoId, DbType.Int32);

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
                $"Erro ao obter o último acesso do aluno ID {alunoId}: {ex.Message}",
                ex);
        }
    }

    // acessos se alternam entre entrada e saída no dia: quantidade ímpar hoje = aluno dentro da academia
    public async Task<bool> EstaNaAcademia(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "SELECT COUNT(*) FROM tb_acesso WHERE pessoa_tipo = @PessoaTipo AND pessoa_id = @AlunoId AND data_hora >= @Inicio AND data_hora < @Fim";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            var hoje = DateTime.Today;

            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            command.AddParameter("@Inicio", hoje, DbType.DateTime);
            command.AddParameter("@Fim", hoje.AddDays(1), DbType.DateTime);

            var total = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));

            return total % 2 == 1;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_ESTA_NA_ACADEMIA",
                $"Erro ao verificar se o aluno ID {alunoId} está na academia: {ex.Message}",
                ex);
        }
    }

    // chave = hora cheia (ex.: 18:00), valor = quantidade de acessos; ordenado do mais para o menos procurado
    public async Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(
        int mes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string hora = GetDateHourExpression("ac.data_hora");
            string mesExpr = GetDateMonthExpression("ac.data_hora");

            string query = $"SELECT {hora} AS hora_acesso, COUNT(*) AS qtd_acessos FROM tb_acesso ac WHERE ac.pessoa_tipo = @PessoaTipo AND {mesExpr} = @Mes GROUP BY {hora} ORDER BY qtd_acessos DESC, hora_acesso ASC";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@Mes", mes, DbType.Int32);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var horarios = new Dictionary<TimeOnly, int>();

            while (await reader.ReadAsync(cancellationToken))
            {
                var horario = new TimeOnly(reader.GetInt32Value("hora_acesso"), 0);
                horarios[horario] = reader.GetInt32Value("qtd_acessos");
            }

            return horarios;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_HORARIO_MAIS_PROCURADO",
                $"Erro ao obter o horário mais procurado do mês {mes}: {ex.Message}",
                ex);
        }
    }

    // chave = id do aluno, valor = permanência média por visita (pares entrada/saída do mês)
    public async Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(
        int mes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string mesExpr = GetDateMonthExpression("ac.data_hora");

            string query = $"SELECT ac.pessoa_id, ac.data_hora FROM tb_acesso ac WHERE ac.pessoa_tipo = @PessoaTipo AND {mesExpr} = @Mes ORDER BY ac.pessoa_id, ac.data_hora";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@Mes", mes, DbType.Int32);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var acessosPorAluno = new Dictionary<int, List<DateTime>>();

            while (await reader.ReadAsync(cancellationToken))
            {
                int alunoId = reader.GetInt32Value("pessoa_id");

                if (!acessosPorAluno.TryGetValue(alunoId, out var lista))
                {
                    lista = [];
                    acessosPorAluno[alunoId] = lista;
                }

                lista.Add(reader.GetDateTimeValue("data_hora"));
            }

            var medias = new Dictionary<int, TimeSpan>();

            foreach (var (alunoId, acessos) in acessosPorAluno)
            {
                var permanencias = AcessoCalculos.CalcularPermanencias(acessos);

                if (permanencias.Count > 0)
                {
                    medias[alunoId] = AcessoCalculos.Media(permanencias);
                }
            }

            return medias;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_PERMANENCIA_MEDIA",
                $"Erro ao obter a permanência média do mês {mes}: {ex.Message}",
                ex);
        }
    }

    // inclui alunos que nunca acessaram a academia
    public async Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(
        int dias,
        CancellationToken cancellationToken = default)
    {
        if (dias < 0)
        {
            throw new InfrastructureException(
                "PARAMETRO_INVALIDO",
                "A quantidade de dias não pode ser negativa.");
        }

        try
        {
            string query = $"{AlunoSelectQuery} WHERE NOT EXISTS (SELECT 1 FROM tb_acesso ac WHERE ac.pessoa_tipo = @PessoaTipo AND ac.pessoa_id = a.id_aluno AND ac.data_hora >= @Limite) ORDER BY a.nome";

            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@Limite", DateTime.Today.AddDays(-dias), DbType.DateTime);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var alunos = new List<Aluno>();

            while (await reader.ReadAsync(cancellationToken))
            {
                alunos.Add(AlunoRepository.Map(reader));
            }

            return alunos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_ALUNOS_SEM_ACESSO",
                $"Erro ao obter alunos sem acesso nos últimos {dias} dias: {ex.Message}",
                ex);
        }
    }
}
