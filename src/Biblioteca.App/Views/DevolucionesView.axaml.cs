using System.Globalization;
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

public partial class DevolucionesView : UserControl
{
    internal record FilaPendiente(int Id, string Libro, string Usuario, DateTime Vence, string Estado, int DiasAtraso, string MultaEstimada);

    private readonly SistemaBiblioteca _sistema;
    private readonly bool _listo;

    public DevolucionesView(SistemaBiblioteca sistema)
    {
        InitializeComponent();
        _sistema = sistema;

        ComboEstadoMulta.ItemsSource = new List<Opcion<bool?>>
        {
            new(null, "Todas las multas"),
            new(false, "Pendientes de pago"),
            new(true, "Pagadas")
        };
        ComboEstadoMulta.SelectedIndex = 0;

        _listo = true;
        RecargarPendientes();
        RecargarMultas();
    }

    private Window Ventana => (Window)TopLevel.GetTopLevel(this)!;
    private FilaPendiente? PendienteSeleccionado => TablaDev.SelectedItem as FilaPendiente;
    private MultaDetalle? MultaSeleccionada => TablaMultas.SelectedItem as MultaDetalle;

    private static string Dinero(decimal monto) => "Q" + monto.ToString("N2", CultureInfo.InvariantCulture);

    private void RecargarPendientes()
    {
        if (!_listo)
            return;

        var filas = _sistema.Prestamos.Listar(soloPendientes: true, texto: CajaBusquedaDev.Text)
            .Select(p => new FilaPendiente(p.Id, p.Libro, p.Usuario, p.FechaVencimiento, p.EstadoTexto, p.DiasAtraso,
                                           p.DiasAtraso > 0 ? Dinero(_sistema.Multas.CalcularMonto(p.DiasAtraso)) : "—"))
            .ToList();
        TablaDev.ItemsSource = filas;
        var atrasados = filas.Count(f => f.DiasAtraso > 0);
        ContadorDev.Text = $"{filas.Count} pendientes de devolución, {atrasados} atrasados";
        BotonDevolver.IsEnabled = PendienteSeleccionado is not null;
    }

    private void RecargarMultas()
    {
        if (!_listo)
            return;

        var pagadas = (ComboEstadoMulta.SelectedItem as Opcion<bool?>)?.Valor;
        var multas = _sistema.Multas.Listar(pagadas, CajaBusquedaMulta.Text);
        TablaMultas.ItemsSource = multas;
        var pendiente = multas.Where(m => !m.Pagada).Sum(m => m.Monto);
        ResumenMultas.Text = $"{multas.Count} multas — pendiente de cobro en esta lista: {Dinero(pendiente)}";
        BotonPagar.IsEnabled = MultaSeleccionada is { Pagada: false };
    }

    private void Pestanas_Cambio(object? sender, SelectionChangedEventArgs e)
    {
        if (e.Source != Pestanas)
            return;
        RecargarPendientes();
        RecargarMultas();
    }

    private void FiltroDev_Cambio(object? sender, RoutedEventArgs e) => RecargarPendientes();
    private void FiltroMulta_Cambio(object? sender, RoutedEventArgs e) => RecargarMultas();
    private void TablaDev_SelectionChanged(object? sender, SelectionChangedEventArgs e) => BotonDevolver.IsEnabled = PendienteSeleccionado is not null;
    private void TablaMultas_SelectionChanged(object? sender, SelectionChangedEventArgs e) => BotonPagar.IsEnabled = MultaSeleccionada is { Pagada: false };

    private void TablaDev_LoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if ((e.Row.DataContext as FilaPendiente)?.DiasAtraso > 0)
            e.Row.Foreground = new SolidColorBrush(Color.Parse("#B3261E"));
        else
            e.Row.ClearValue(TemplatedControl.ForegroundProperty);
    }

    private void TablaMultas_LoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if ((e.Row.DataContext as MultaDetalle)?.Pagada == true)
            e.Row.Foreground = new SolidColorBrush(Color.Parse("#7A8593"));
        else
            e.Row.ClearValue(TemplatedControl.ForegroundProperty);
    }

    private async void Devolver_Click(object? sender, RoutedEventArgs e)
    {
        if (PendienteSeleccionado is not { } fila)
            return;

        var aviso = fila.DiasAtraso > 0
            ? $"\n\nTiene {fila.DiasAtraso} día(s) de atraso: se generará una multa de {fila.MultaEstimada}."
            : string.Empty;
        if (!await Dialogos.Confirmar(Ventana,
                $"¿Registrar la devolución de «{fila.Libro}» entregado por {fila.Usuario}?{aviso}", "Registrar devolución"))
            return;

        try
        {
            var resultado = _sistema.Devoluciones.Registrar(fila.Id);
            RecargarPendientes();
            RecargarMultas();
            var mensaje = $"«{fila.Libro}» fue devuelto y el ejemplar quedó disponible.";
            if (resultado.Multa is { } multa)
                mensaje += $"\n\nSe generó una multa de {Dinero(multa.Monto)} por {multa.DiasAtraso} día(s) de atraso. " +
                           "Mientras no se pague, el usuario no podrá solicitar nuevos préstamos.";
            await Dialogos.Informacion(Ventana, mensaje, "Devolución registrada");
        }
        catch (Exception ex) when (ex is ValidacionException or AlmacenamientoException)
        {
            await Dialogos.Error(Ventana, ex.Message);
        }
    }

    private async void Pagar_Click(object? sender, RoutedEventArgs e)
    {
        if (MultaSeleccionada is not { Pagada: false } multa)
            return;

        if (!await Dialogos.Confirmar(Ventana,
                $"¿Registrar el pago de {multa.MontoTexto} de {multa.Usuario}?", "Registrar pago de multa"))
            return;

        try
        {
            _sistema.Multas.Pagar(multa.Id);
            RecargarMultas();
            await Dialogos.Informacion(Ventana, $"Se registró el pago de {multa.MontoTexto} de {multa.Usuario}.", "Pago registrado");
        }
        catch (Exception ex) when (ex is ValidacionException or AlmacenamientoException)
        {
            await Dialogos.Error(Ventana, ex.Message);
        }
    }
}
