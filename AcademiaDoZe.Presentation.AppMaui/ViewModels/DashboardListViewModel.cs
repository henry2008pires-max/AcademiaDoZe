using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IMatriculaService _matriculaService;

    private int _totalLogradouros;
    public int TotalLogradouros { get => _totalLogradouros; set => SetProperty(ref _totalLogradouros, value); }

    private int _totalAlunos;
    public int TotalAlunos { get => _totalAlunos; set => SetProperty(ref _totalAlunos, value); }

    private int _totalColaboradores;
    public int TotalColaboradores { get => _totalColaboradores; set => SetProperty(ref _totalColaboradores, value); }

    private int _totalMatriculas;
    public int TotalMatriculas { get => _totalMatriculas; set => SetProperty(ref _totalMatriculas, value); }

    public DashboardListViewModel(
        ILogradouroService logradouroService,
        IAlunoService alunoService,
        IColaboradorService colaboradorService,
        IMatriculaService matriculaService)
    {
        _logradouroService = logradouroService;
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _matriculaService = matriculaService;
        Title = "Painel Principal";
    }

    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var logradourosTask = _logradouroService.ObterTodosAsync(cts.Token);
            var alunosTask = _alunoService.ObterTodosAsync(cts.Token);
            var colaboradoresTask = _colaboradorService.ObterTodosAsync(cts.Token);
            var matriculasTask = _matriculaService.ObterTodasAsync(cts.Token);

            await Task.WhenAll(logradourosTask, alunosTask, colaboradoresTask, matriculasTask);

            TotalLogradouros = (await logradourosTask)?.Count() ?? 0;
            TotalAlunos = (await alunosTask)?.Count() ?? 0;
            TotalColaboradores = (await colaboradoresTask)?.Count() ?? 0;
            TotalMatriculas = (await matriculasTask)?.Count() ?? 0;
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlert("Tempo Esgotado", "Expirou o tempo de resposta com o banco de dados.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Falha ao carregar métricas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToLogradourosAsync() => await Shell.Current.GoToAsync("//logradouros");

    [RelayCommand]
    private async Task NavigateToAlunosAsync() => await Shell.Current.GoToAsync("//alunos");

    [RelayCommand]
    private async Task NavigateToColaboradoresAsync() => await Shell.Current.GoToAsync("//colaboradores");

    [RelayCommand]
    private async Task NavigateToMatriculasAsync() => await Shell.Current.GoToAsync("//matriculas");
}
