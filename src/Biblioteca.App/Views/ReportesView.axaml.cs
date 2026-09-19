using System.Text;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Biblioteca.App.Servicios;
using Biblioteca.Core;
using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Servicios;

namespace Biblioteca.App.Views;

public partial class ReportesView : UserControl
{
    private readonly SistemaBiblioteca _sistema;
    private ReporteTabular? _actual;

    public ReportesView(SistemaBiblioteca sistema)
    {
        InitializeComponent();
        _sistema = sistema;

        ComboReporte.ItemsSource = new List<Opcion<Func<ReporteTabular>>>
        {
            new(sistema.Reportes.LibrosDisponibles, "Libros disponibles"),
            new(sistema.Reportes.PrestamosActivos, "Préstamos activos"),
            new(sistema.Reportes.LibrosAtrasados, "Libros atrasados"),
            new(sistema.Reportes.Usuarios, "Usuarios"),
            new(sistema.Reportes.Multas, "Multas")
        };
        ComboReporte.SelectedIndex = 0;
    }

    private Window Ventana => (Window)TopLevel.GetTopLevel(this)!;

    private void Reporte_Cambio(object? sender, SelectionChangedEventArgs e)
    {
        if (ComboReporte.SelectedItem is not Opcion<Func<ReporteTabular>> opcion)
            return;

        _actual = opcion.Valor();
        Tabla.Columns.Clear();
        for (var i = 0; i < _actual.Columnas.Count; i++)
        {
            Tabla.Columns.Add(new DataGridTextColumn
            {
                Header = _actual.Columnas[i],
                Binding = new Binding($"[{i}]"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
        }

        Tabla.ItemsSource = _actual.Filas;
        Resumen.Text = _actual.Resumen;
    }

    private async void Exportar_Click(object? sender, RoutedEventArgs e)
    {
        if (_actual is null)
            return;

        try
        {
            var archivo = await Ventana.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Exportar reporte",
                SuggestedFileName = "reporte-" + _actual.Titulo.ToLowerInvariant().Replace(' ', '-') + ".csv",
                DefaultExtension = "csv",
                FileTypeChoices = new[] { new FilePickerFileType("Archivo CSV") { Patterns = new[] { "*.csv" } } }
            });
            if (archivo is null)
                return;

            await using (var flujo = await archivo.OpenWriteAsync())
            {
                flujo.SetLength(0);
                await using var escritor = new StreamWriter(flujo, new UTF8Encoding(true));
                await escritor.WriteAsync(_actual.ACsv());
            }

            await Dialogos.Informacion(Ventana, $"El reporte «{_actual.Titulo}» se exportó correctamente.", "Reporte exportado");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or AlmacenamientoException)
        {
            await Dialogos.Error(Ventana, "No se pudo guardar el archivo: " + ex.Message);
        }
    }
}
