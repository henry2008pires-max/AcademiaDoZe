using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroListPage : ContentPage
{
    public LogradouroListPage(LogradouroListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LogradouroListViewModel viewModel)
        {
            await viewModel.LoadLogradourosCommand.ExecuteAsync(null);
        }
    }

    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is LogradouroDto dto && BindingContext is LogradouroListViewModel vm)
        {
            await vm.EditLogradouroCommand.ExecuteAsync(dto);
        }
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is LogradouroDto dto && BindingContext is LogradouroListViewModel vm)
        {
            await vm.DeleteLogradouroCommand.ExecuteAsync(dto);
        }
    }
}
