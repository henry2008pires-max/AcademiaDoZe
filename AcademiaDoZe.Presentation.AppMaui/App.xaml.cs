using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui
{
    // Application conflita com o nome da nossa camada de aplicação:
    // usamos o namespace completo para apontar para a classe Application do MAUI.
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();

            // aplicar o tema salvo nas preferências
            AplicarTema();

            // toda vez que o usuário salvar o tema, essa mensagem será enviada e o tema atualizado
            WeakReferenceMessenger.Default.Register<TemaPreferencesUpdatedMessage>(this, (r, m) =>
            {
                // m.Value contém o valor enviado na mensagem
                AplicarTema();
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private void AplicarTema()
        {
            UserAppTheme = Preferences.Get("Tema", "system") switch
            {
                "light" => AppTheme.Light,
                "dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified,
            };
        }
    }
}
