using Avalonia.Controls;
using Avalonia.Interactivity;
using Biblioteca.App.Servicios;
using Biblioteca.Core;

namespace Biblioteca.App.Views;

public partial class MainWindow : Window
{
    private readonly SistemaBiblioteca _sistema;
    private readonly Button[] _botonesMenu;

    public MainWindow(SistemaBiblioteca sistema)
    {
        InitializeComponent();
        _sistema = sistema;
        _botonesMenu = new[] { NavInicio, NavLibros, NavUsuarios, NavPrestamos, NavDevoluciones, NavReportes };
        Navegar("inicio");
    }

    public void Navegar(string destino)
    {
        Contenido.Content = destino switch
        {
            "libros" => new LibrosView(_sistema),
            "usuarios" => new UsuariosView(_sistema),
            "prestamos" => new PrestamosView(_sistema),
            "devoluciones" => new DevolucionesView(_sistema),
            "reportes" => new ReportesView(_sistema),
            _ => new InicioView(_sistema, Navegar)
        };

        foreach (var boton in _botonesMenu)
            boton.Classes.Set("activo", (string?)boton.Tag == destino);
    }

    private void Nav_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string destino })
            Navegar(destino);
    }

    private async void Salir_Click(object? sender, RoutedEventArgs e)
    {
        if (await Dialogos.Confirmar(this, "¿Desea cerrar el sistema de biblioteca?\nLos datos ya están guardados en los archivos JSON.", "Salir"))
            Close();
    }
}
