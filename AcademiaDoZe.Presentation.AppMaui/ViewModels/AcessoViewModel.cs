using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

// item da lista "Acessos Registrados Hoje"
public class AcessoLogItem
{
    public required string Nome { get; init; }
    public required string Mensagem { get; init; }
    public required string Hora { get; init; }
    public required bool IsEntrada { get; init; }
    public bool IsSaida => !IsEntrada;
}

// Tela de Controle de Acesso (catraca): entrada/saída de alunos e colaboradores
public partial class AcessoViewModel : BaseViewModel
{
    // limite de horas diárias por vínculo do colaborador
    private static readonly TimeSpan LimiteClt = TimeSpan.FromHours(8);
    private static readonly TimeSpan LimiteEstagio = TimeSpan.FromHours(6);

    private readonly IAcessoAlunoService _acessoAlunoService;
    private readonly IAcessoColaboradorService _acessoColaboradorService;
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IMatriculaService _matriculaService;

    private List<PessoaDto> _todasPessoas = [];

    public AcessoViewModel(
        IAcessoAlunoService acessoAlunoService,
        IAcessoColaboradorService acessoColaboradorService,
        IAlunoService alunoService,
        IColaboradorService colaboradorService,
        IMatriculaService matriculaService)
    {
        _acessoAlunoService = acessoAlunoService;
        _acessoColaboradorService = acessoColaboradorService;
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _matriculaService = matriculaService;
        Title = "Controle de Acesso";
    }

    #region Modo (Aluno / Colaborador)

    private bool _isModoAluno = true;
    public bool IsModoAluno
    {
        get => _isModoAluno;
        private set
        {
            if (SetProperty(ref _isModoAluno, value))
            {
                OnPropertyChanged(nameof(IsModoColaborador));
                OnPropertyChanged(nameof(ModoAtualTexto));
                OnPropertyChanged(nameof(PessoaLabel));
            }
        }
    }

    public bool IsModoColaborador => !IsModoAluno;
    public string ModoAtualTexto => $"Modo atual selecionado: {(IsModoAluno ? "Aluno" : "Colaborador")}";
    public string PessoaLabel => IsModoAluno ? "Selecione o Aluno:" : "Selecione o Colaborador:";

    #endregion

    #region Busca e seleção

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                AplicarFiltro();
        }
    }

    public ObservableCollection<PessoaDto> Pessoas { get; } = [];

    private PessoaDto? _selectedPessoa;
    public PessoaDto? SelectedPessoa
    {
        get => _selectedPessoa;
        set
        {
            if (SetProperty(ref _selectedPessoa, value))
            {
                OnPropertyChanged(nameof(HasPessoa));
                OnPropertyChanged(nameof(HasNoPessoa));
                _ = AtualizarStatusAsync();
            }
        }
    }

    public bool HasPessoa => SelectedPessoa != null;
    public bool HasNoPessoa => SelectedPessoa == null;

    private bool _dentroDaAcademia;
    public bool DentroDaAcademia
    {
        get => _dentroDaAcademia;
        private set
        {
            if (SetProperty(ref _dentroDaAcademia, value))
            {
                OnPropertyChanged(nameof(ForaDaAcademia));
                OnPropertyChanged(nameof(AcaoTexto));
            }
        }
    }

    public bool ForaDaAcademia => !DentroDaAcademia;
    public string AcaoTexto => DentroDaAcademia ? "Registrar Saída" : "Registrar Entrada";

    #endregion

    #region Resultado (mensagem de liberação / negação)

    private bool _temResultado;
    private bool _resultadoSucesso;
    private string _resultadoTitulo = string.Empty;
    private string _resultadoMensagem = string.Empty;

    public bool ResultadoSucesso => _temResultado && _resultadoSucesso;
    public bool ResultadoErro => _temResultado && !_resultadoSucesso;

    public string ResultadoTitulo
    {
        get => _resultadoTitulo;
        private set => SetProperty(ref _resultadoTitulo, value);
    }

    public string ResultadoMensagem
    {
        get => _resultadoMensagem;
        private set => SetProperty(ref _resultadoMensagem, value);
    }

    private void MostrarResultado(bool sucesso, string titulo, string mensagem)
    {
        _temResultado = true;
        _resultadoSucesso = sucesso;
        ResultadoTitulo = titulo;
        ResultadoMensagem = mensagem;
        OnPropertyChanged(nameof(ResultadoSucesso));
        OnPropertyChanged(nameof(ResultadoErro));
    }

    private void LimparResultado()
    {
        _temResultado = false;
        OnPropertyChanged(nameof(ResultadoSucesso));
        OnPropertyChanged(nameof(ResultadoErro));
    }

    #endregion

    public ObservableCollection<AcessoLogItem> AcessosHoje { get; } = [];

    public async Task InitializeAsync()
    {
        await CarregarPessoasAsync();
        await CarregarAcessosHojeAsync();
    }

    [RelayCommand]
    private async Task SelecionarModoAlunoAsync() => await TrocarModoAsync(true);

    [RelayCommand]
    private async Task SelecionarModoColaboradorAsync() => await TrocarModoAsync(false);

    private async Task TrocarModoAsync(bool aluno)
    {
        if (IsBusy || IsModoAluno == aluno)
            return;

        IsModoAluno = aluno;
        _todasPessoas = [];
        SearchText = string.Empty;
        SelectedPessoa = null;
        LimparResultado();
        await CarregarPessoasAsync();
    }

    private async Task CarregarPessoasAsync()
    {
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            if (IsModoAluno)
            {
                var alunos = await _alunoService.ObterTodosAsync(cts.Token);
                _todasPessoas = alunos.Cast<PessoaDto>().ToList();
            }
            else
            {
                var colaboradores = await _colaboradorService.ObterTodosAsync(cts.Token);
                _todasPessoas = colaboradores.Cast<PessoaDto>().ToList();
            }

            AplicarFiltro();
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento da lista expirou. Verifique a conexão com o banco.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar pessoas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // filtra a lista pelo nome ou por parte do CPF (somente dígitos)
    private void AplicarFiltro()
    {
        var termo = SearchText?.Trim() ?? string.Empty;
        var digitos = new string(termo.Where(char.IsDigit).ToArray());

        IEnumerable<PessoaDto> filtradas = _todasPessoas;
        if (termo.Length > 0)
        {
            filtradas = _todasPessoas.Where(p =>
                p.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                (digitos.Length > 0 && new string(p.Cpf.Where(char.IsDigit).ToArray()).Contains(digitos)));
        }

        var lista = filtradas.OrderBy(p => p.Nome).ToList();
        var anterior = SelectedPessoa;

        Pessoas.Clear();
        foreach (var p in lista)
            Pessoas.Add(p);

        var mantida = anterior == null ? null : Pessoas.FirstOrDefault(p => p.Id == anterior.Id);

        if (mantida != null)
            SelectedPessoa = mantida;
        else if (termo.Length > 0 && Pessoas.Count == 1)
            SelectedPessoa = Pessoas[0]; // um único resultado: já seleciona
        else
            SelectedPessoa = null;
    }

    private async Task<bool> EstaDentroAsync(PessoaDto pessoa, CancellationToken ct)
    {
        if (IsModoAluno)
            return await _acessoAlunoService.EstaNaAcademia(pessoa.Id, ct);

        // colaborador: acessos do dia se alternam entre entrada e saída (quantidade ímpar = dentro)
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var acessos = await _acessoColaboradorService.ObterAcessosPorColaboradorPeriodo(pessoa.Id, hoje, hoje, ct);
        return acessos.Count() % 2 == 1;
    }

    private async Task AtualizarStatusAsync()
    {
        var pessoa = SelectedPessoa;
        if (pessoa == null)
        {
            DentroDaAcademia = false;
            return;
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var dentro = await EstaDentroAsync(pessoa, cts.Token);

            // ignora resultado se a seleção mudou durante a consulta
            if (ReferenceEquals(pessoa, SelectedPessoa))
                DentroDaAcademia = dentro;
        }
        catch
        {
            DentroDaAcademia = false;
        }
    }

    [RelayCommand]
    private async Task RegistrarAcessoAsync()
    {
        if (IsBusy)
            return;

        var pessoa = SelectedPessoa;
        if (pessoa == null)
        {
            MostrarResultado(false, "Selecione uma pessoa", "Busque e selecione um aluno ou colaborador antes de registrar o acesso.");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // lê o estado atual direto do banco (evita registrar duas entradas seguidas)
            var dentro = await EstaDentroAsync(pessoa, cts.Token);

            if (IsModoAluno)
                await RegistrarAlunoAsync(pessoa, dentro, cts.Token);
            else
                await RegistrarColaboradorAsync(pessoa, dentro, cts.Token);
        }
        catch (OperationCanceledException)
        {
            MostrarResultado(false, "Tempo Esgotado", "O registro do acesso expirou. Verifique a conexão com o banco.");
        }
        catch (Exception ex)
        {
            MostrarResultado(false, "Acesso Negado", TraduzirErro(ex));
        }
        finally
        {
            IsBusy = false;
        }

        await AtualizarStatusAsync();
        await CarregarAcessosHojeAsync();
    }

    private async Task RegistrarAlunoAsync(PessoaDto aluno, bool dentro, CancellationToken ct)
    {
        if (!dentro)
        {
            // valida matrícula ativa e horário (regras no AcessoAlunoService)
            await _acessoAlunoService.RegistrarAcesso(aluno.Id, null, ct);

            // na entrada, mostra quanto tempo ainda tem de plano
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var matriculas = await _matriculaService.ObterAtivasAsync(aluno.Id, ct);
            var ativa = matriculas.Where(m => m.DataFim >= hoje).OrderByDescending(m => m.DataFim).FirstOrDefault();
            var dias = ativa == null ? 0 : ativa.DataFim.DayNumber - hoje.DayNumber;

            MostrarResultado(true, "Entrada Autorizada", $"Entrada autorizada. Dias restantes de plano: {dias} dia(s).");
        }
        else
        {
            var entrada = await _acessoAlunoService.ObterUltimoAcesso(aluno.Id, ct);
            var saida = await _acessoAlunoService.RegistrarAcesso(aluno.Id, null, ct);

            // na saída, mostra o tempo que permaneceu na academia
            var permanencia = entrada == null ? TimeSpan.Zero : saida.DataHora - entrada.DataHora;

            MostrarResultado(true, "Saída Registrada", $"Saída registrada. Tempo de permanência: {Formatar(permanencia)}.");
        }
    }

    private async Task RegistrarColaboradorAsync(PessoaDto pessoa, bool dentro, CancellationToken ct)
    {
        var colaborador = pessoa as ColaboradorDto
            ?? throw new InvalidOperationException("A pessoa selecionada não é um colaborador.");

        var hoje = DateOnly.FromDateTime(DateTime.Today);

        if (!dentro)
        {
            // valida o limite diário: 8 horas (CLT) ou 6 horas (estágio)
            var trabalhadas = await _acessoColaboradorService.ObterHorasTrabalhadasNoDia(colaborador.Id, hoje, ct);
            var limite = colaborador.Vinculo == AppColaboradorVinculo.CLT ? LimiteClt : LimiteEstagio;

            if (trabalhadas >= limite)
            {
                MostrarResultado(false, "Acesso Negado",
                    $"Acesso negado: o limite diário de {limite.TotalHours:0} horas ({colaborador.Vinculo.GetDisplayName()}) já foi atingido. Horas trabalhadas hoje: {Formatar(trabalhadas)}.");
                return;
            }

            await _acessoColaboradorService.RegistrarAcesso(colaborador.Id, null, ct);

            MostrarResultado(true, "Entrada Autorizada", $"Entrada registrada. Horas trabalhadas hoje até o momento: {Formatar(trabalhadas)}.");
        }
        else
        {
            await _acessoColaboradorService.RegistrarAcesso(colaborador.Id, null, ct);

            // soma de todos os registros (pares entrada/saída) do dia
            var total = await _acessoColaboradorService.ObterHorasTrabalhadasNoDia(colaborador.Id, hoje, ct);

            MostrarResultado(true, "Saída Registrada", $"Saída registrada. Tempo de permanência hoje (soma de todos os registros): {Formatar(total)}.");
        }
    }

    [RelayCommand]
    private async Task AtualizarAcessosAsync()
    {
        await CarregarAcessosHojeAsync();
        await AtualizarStatusAsync();
    }

    // monta a lista de acessos do dia (alunos e colaboradores): 1º registro = entrada, 2º = saída, 3º = entrada...
    private async Task CarregarAcessosHojeAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var hoje = DateOnly.FromDateTime(DateTime.Today);

            var acessosAlunos = (await _acessoAlunoService.ObterAcessosPorAlunoPeriodo(null, hoje, hoje, cts.Token)).ToList();
            var acessosColabs = (await _acessoColaboradorService.ObterAcessosPorColaboradorPeriodo(null, hoje, hoje, cts.Token)).ToList();

            var nomesAlunos = new Dictionary<int, string>();
            foreach (var id in acessosAlunos.Select(a => a.AlunoId).Distinct())
            {
                var a = await _alunoService.ObterPorIdAsync(id, cts.Token);
                nomesAlunos[id] = a?.Nome ?? $"Aluno #{id}";
            }

            var nomesColabs = new Dictionary<int, string>();
            foreach (var id in acessosColabs.Select(a => a.ColaboradorId).Distinct())
            {
                var c = await _colaboradorService.ObterPorIdAsync(id, cts.Token);
                nomesColabs[id] = c?.Nome ?? $"Colaborador #{id}";
            }

            var itens = new List<(DateTime Hora, AcessoLogItem Item)>();

            foreach (var grupo in acessosAlunos.GroupBy(a => a.AlunoId))
            {
                var ordenados = grupo.OrderBy(a => a.DataHora).ToList();
                for (int i = 0; i < ordenados.Count; i++)
                {
                    bool entrada = i % 2 == 0;
                    var msg = entrada
                        ? "Entrada registrada."
                        : $"Saída registrada. Tempo de permanência: {Formatar(ordenados[i].DataHora - ordenados[i - 1].DataHora)}.";

                    itens.Add((ordenados[i].DataHora, new AcessoLogItem
                    {
                        Nome = nomesAlunos[grupo.Key],
                        Mensagem = msg,
                        Hora = ordenados[i].DataHora.ToString("HH:mm:ss"),
                        IsEntrada = entrada
                    }));
                }
            }

            foreach (var grupo in acessosColabs.GroupBy(a => a.ColaboradorId))
            {
                var ordenados = grupo.OrderBy(a => a.DataHora).ToList();
                var acumulado = TimeSpan.Zero;
                for (int i = 0; i < ordenados.Count; i++)
                {
                    bool entrada = i % 2 == 0;
                    string msg;
                    if (entrada)
                    {
                        msg = $"Entrada registrada. Horas trabalhadas hoje até o momento: {Formatar(acumulado)}.";
                    }
                    else
                    {
                        acumulado += ordenados[i].DataHora - ordenados[i - 1].DataHora;
                        msg = $"Saída registrada. Horas trabalhadas hoje: {Formatar(acumulado)}.";
                    }

                    itens.Add((ordenados[i].DataHora, new AcessoLogItem
                    {
                        Nome = nomesColabs[grupo.Key],
                        Mensagem = msg,
                        Hora = ordenados[i].DataHora.ToString("HH:mm:ss"),
                        IsEntrada = entrada
                    }));
                }
            }

            AcessosHoje.Clear();
            foreach (var (_, item) in itens.OrderByDescending(i => i.Hora))
                AcessosHoje.Add(item);
        }
        catch (OperationCanceledException)
        {
            // lista não atualizada por tempo esgotado; o botão Atualizar tenta de novo
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar acessos de hoje: {ex.Message}", "OK");
        }
    }

    // converte os códigos de erro do domínio em mensagens amigáveis
    private static string TraduzirErro(Exception ex)
    {
        var msg = ex.Message ?? string.Empty;

        if (msg.Contains("MATRICULA_ATIVA_NAO_ENCONTRADA"))
            return "Acesso negado: o aluno não possui matrícula ativa.";
        if (msg.Contains("DATA_HORA_INTERVALO_INVALIDO"))
            return "Acesso negado: a academia só registra acessos entre 06:00 e 22:00.";
        if (msg.Contains("ALUNO_NAO_ENCONTRADO"))
            return "Aluno não encontrado.";
        if (msg.Contains("COLABORADOR_NAO_ENCONTRADO"))
            return "Colaborador não encontrado.";

        return $"Não foi possível registrar o acesso: {msg}";
    }

    // hh:mm:ss (as horas podem passar de 24 na soma, por isso não usa TimeSpan.ToString)
    private static string Formatar(TimeSpan t)
        => $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
}
