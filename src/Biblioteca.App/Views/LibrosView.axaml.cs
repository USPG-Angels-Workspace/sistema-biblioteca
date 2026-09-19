using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Biblioteca.App.Servicios;
using Biblioteca.Core;
using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Modelos;

namespace Biblioteca.App.Views;

public partial class LibrosView : UserControl
{
    private readonly SistemaBiblioteca _sistema;
    private readonly bool _listo;

    public LibrosView(SistemaBiblioteca sistema)
    {
        InitializeComponent();
        _sistema = sistema;

        var categorias = new List<Opcion<CategoriaLibro?>> { new(null, "Todas las categorías") };
        categorias.AddRange(Enum.GetValues<CategoriaLibro>().Select(c => new Opcion<CategoriaLibro?>(c, c.Texto())));
        ComboCategoria.ItemsSource = categorias;
        ComboCategoria.SelectedIndex = 0;

        _listo = true;
        Recargar();
    }

    private Window Ventana => (Window)TopLevel.GetTopLevel(this)!;
    private Libro? Seleccionado => Tabla.SelectedItem as Libro;

    private void Recargar()
    {
        if (!_listo)
            return;

        var categoria = (ComboCategoria.SelectedItem as Opcion<CategoriaLibro?>)?.Valor;
        var libros = _sistema.Libros.Buscar(CajaBusqueda.Text, categoria, CheckDisponibles.IsChecked == true);
        Tabla.ItemsSource = libros;
        Contador.Text = libros.Count == 1 ? "1 libro" : $"{libros.Count} libros";
        ActualizarBotones();
    }

    private void ActualizarBotones()
    {
        BotonEditar.IsEnabled = Seleccionado is not null;
        BotonEliminar.IsEnabled = Seleccionado is not null;
    }

    private void Filtro_Cambio(object? sender, RoutedEventArgs e) => Recargar();

    private void Tabla_SelectionChanged(object? sender, SelectionChangedEventArgs e) => ActualizarBotones();

    private async void Tabla_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if ((e.Source as Visual)?.FindAncestorOfType<DataGridRow>() is not null)
            await EditarSeleccionado();
    }

    private async void Nuevo_Click(object? sender, RoutedEventArgs e)
    {
        var formulario = new LibroFormWindow(_sistema);
        if (await formulario.ShowDialog<bool>(Ventana))
        {
            Recargar();
            await Dialogos.Informacion(Ventana, formulario.MensajeExito, "Libro registrado");
        }
    }

    private async void Editar_Click(object? sender, RoutedEventArgs e) => await EditarSeleccionado();

    private async Task EditarSeleccionado()
    {
        if (Seleccionado is not { } libro)
            return;

        var formulario = new LibroFormWindow(_sistema, libro);
        if (await formulario.ShowDialog<bool>(Ventana))
        {
            Recargar();
            await Dialogos.Informacion(Ventana, formulario.MensajeExito, "Libro actualizado");
        }
    }

    private async void Eliminar_Click(object? sender, RoutedEventArgs e)
    {
        if (Seleccionado is not { } libro)
            return;

        if (!await Dialogos.Confirmar(Ventana, $"¿Eliminar el libro «{libro.Titulo}»?\nEsta acción no se puede deshacer.", "Eliminar libro"))
            return;

        try
        {
            _sistema.Libros.Eliminar(libro.Id);
            Recargar();
            await Dialogos.Informacion(Ventana, $"El libro «{libro.Titulo}» fue eliminado.", "Libro eliminado");
        }
        catch (Exception ex) when (ex is ValidacionException or AlmacenamientoException)
        {
            await Dialogos.Error(Ventana, ex.Message);
        }
    }
}
