using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Biblioteca.App.Servicios;
using Biblioteca.Core;
using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Modelos;
using Biblioteca.Core.Servicios;

namespace Biblioteca.App.Views;

public partial class PrestamosView : UserControl
{
    private readonly SistemaBiblioteca _sistema;
    private readonly bool _listo;

    public PrestamosView(SistemaBiblioteca sistema)
    {
        InitializeComponent();
        _sistema = sistema;

        ComboEstado.ItemsSource = new List<Opcion<EstadoPrestamo?>>
        {
            new(null, "Todos los estados"),
            new(EstadoPrestamo.Activo, "Activos"),
            new(EstadoPrestamo.Atrasado, "Atrasados"),
            new(EstadoPrestamo.Devuelto, "Devueltos")
        };
        ComboEstado.SelectedIndex = 0;

        _listo = true;
        Recargar();
    }

    private Window Ventana => (Window)TopLevel.GetTopLevel(this)!;
    private PrestamoDetalle? Seleccionado => Tabla.SelectedItem as PrestamoDetalle;

    private void Recargar()
    {
        if (!_listo)
            return;

        var estado = (ComboEstado.SelectedItem as Opcion<EstadoPrestamo?>)?.Valor;
        var prestamos = _sistema.Prestamos.Listar(texto: CajaBusqueda.Text, estado: estado);
        Tabla.ItemsSource = prestamos;
        Contador.Text = prestamos.Count == 1 ? "1 préstamo" : $"{prestamos.Count} préstamos";
        ActualizarBotones();
    }

    private void ActualizarBotones() =>
        BotonRenovar.IsEnabled = Seleccionado is { Estado: EstadoPrestamo.Activo };

    private void Filtro_Cambio(object? sender, RoutedEventArgs e) => Recargar();

    private void Tabla_SelectionChanged(object? sender, SelectionChangedEventArgs e) => ActualizarBotones();

    private void Tabla_LoadingRow(object? sender, DataGridRowEventArgs e)
    {
        switch ((e.Row.DataContext as PrestamoDetalle)?.Estado)
        {
            case EstadoPrestamo.Atrasado:
                e.Row.Foreground = new SolidColorBrush(Color.Parse("#B3261E"));
                break;
            case EstadoPrestamo.Devuelto:
                e.Row.Foreground = new SolidColorBrush(Color.Parse("#7A8593"));
                break;
            default:
                e.Row.ClearValue(TemplatedControl.ForegroundProperty);
                break;
        }
    }

    private async void Nuevo_Click(object? sender, RoutedEventArgs e)
    {
        var formulario = new PrestamoFormWindow(_sistema);
        if (await formulario.ShowDialog<bool>(Ventana))
        {
            Recargar();
            await Dialogos.Informacion(Ventana, formulario.MensajeExito, "Préstamo registrado");
        }
    }

    private async void Renovar_Click(object? sender, RoutedEventArgs e)
    {
        if (Seleccionado is not { } seleccionado)
            return;

        if (!await Dialogos.Confirmar(Ventana,
                $"¿Renovar el préstamo de «{seleccionado.Libro}» a nombre de {seleccionado.Usuario}?", "Renovar préstamo"))
            return;

        try
        {
            var prestamo = _sistema.Prestamos.Renovar(seleccionado.Id);
            Recargar();
            await Dialogos.Informacion(Ventana,
                $"Préstamo renovado. Nueva fecha de devolución: {prestamo.FechaVencimiento:dd/MM/yyyy} " +
                $"(renovación {prestamo.Renovaciones} de {Prestamo.MaximoRenovaciones}).", "Préstamo renovado");
        }
        catch (Exception ex) when (ex is ValidacionException or AlmacenamientoException)
        {
            await Dialogos.Error(Ventana, ex.Message);
        }
    }
}
