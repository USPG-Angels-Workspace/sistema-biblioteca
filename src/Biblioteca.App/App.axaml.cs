using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Biblioteca.App.Servicios;
using Biblioteca.App.Views;
using Biblioteca.Core;
using Biblioteca.Core.Excepciones;

namespace Biblioteca.App;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime escritorio)
        {
            var carpeta = Contexto.CarpetaDatos();
            try
            {
                escritorio.MainWindow = new MainWindow(new SistemaBiblioteca(carpeta));
            }
            catch (AlmacenamientoException ex)
            {
                escritorio.MainWindow = new DialogoMensajeWindow(
                    "No se pudo iniciar el sistema",
                    $"{ex.Message}\n\nCarpeta de datos: {carpeta}",
                    TipoDialogo.Error, conCancelar: false);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
