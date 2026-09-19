using Avalonia.Controls;
using Avalonia.Interactivity;
using Biblioteca.App.Servicios;
using Biblioteca.Core;
using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Modelos;

namespace Biblioteca.App.Views;

public partial class LibroFormWindow : Window
{
    private readonly SistemaBiblioteca _sistema;
    private readonly Libro? _libro;

    public string MensajeExito { get; private set; } = string.Empty;

    public LibroFormWindow()
    {
        InitializeComponent();
        _sistema = null!;
    }

    public LibroFormWindow(SistemaBiblioteca sistema, Libro? libro = null) : this()
    {
        _sistema = sistema;
        _libro = libro;

        ComboCategoria.ItemsSource = Enum.GetValues<CategoriaLibro>().Select(c => new Opcion<CategoriaLibro>(c, c.Texto())).ToList();
        ComboCategoria.SelectedIndex = 0;

        Title = Encabezado.Text = libro is null ? "Nuevo libro" : "Editar libro";
        if (libro is null)
            return;

        CajaIsbn.Text = libro.Isbn;
        CajaTitulo.Text = libro.Titulo;
        CajaAutor.Text = libro.Autor;
        CajaEditorial.Text = libro.Editorial;
        CajaAnio.Text = libro.Anio.ToString();
        CajaCopias.Text = libro.CopiasTotales.ToString();
        ComboCategoria.SelectedIndex = (int)libro.Categoria;
        Estado.Text = $"Ejemplares en préstamo actualmente: {libro.CopiasPrestadas}. El total no puede ser menor.";
        Estado.IsVisible = true;
    }

    private void Guardar_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (!int.TryParse(CajaAnio.Text?.Trim(), out var anio))
                throw new ValidacionException("El año de publicación debe ser un número entero.");
            if (!int.TryParse(CajaCopias.Text?.Trim(), out var copias))
                throw new ValidacionException("La cantidad de ejemplares debe ser un número entero.");
            var categoria = (ComboCategoria.SelectedItem as Opcion<CategoriaLibro>)?.Valor
                ?? throw new ValidacionException("Seleccione una categoría.");

            var libro = _libro is null
                ? _sistema.Libros.Registrar(CajaIsbn.Text ?? "", CajaTitulo.Text ?? "", CajaAutor.Text ?? "",
                                            CajaEditorial.Text ?? "", anio, categoria, copias)
                : _sistema.Libros.Editar(_libro.Id, CajaIsbn.Text ?? "", CajaTitulo.Text ?? "", CajaAutor.Text ?? "",
                                         CajaEditorial.Text ?? "", anio, categoria, copias);

            MensajeExito = _libro is null
                ? $"El libro «{libro.Titulo}» se registró correctamente con {libro.CopiasTotales} ejemplar(es)."
                : $"Los datos de «{libro.Titulo}» se actualizaron correctamente.";
            Close(true);
        }
        catch (Exception ex) when (ex is ValidacionException or AlmacenamientoException)
        {
            MensajeError.Text = ex.Message;
            MensajeError.IsVisible = true;
        }
    }

    private void Cancelar_Click(object? sender, RoutedEventArgs e) => Close(false);
}
