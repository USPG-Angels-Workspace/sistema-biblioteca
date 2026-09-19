using Avalonia.Controls;
using Avalonia.Interactivity;
using Biblioteca.App.Servicios;
using Biblioteca.Core;
using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Modelos;

namespace Biblioteca.App.Views;

public partial class PrestamoFormWindow : Window
{
    private readonly SistemaBiblioteca _sistema;

    public string MensajeExito { get; private set; } = string.Empty;

    public PrestamoFormWindow()
    {
        InitializeComponent();
        _sistema = null!;
    }

    public PrestamoFormWindow(SistemaBiblioteca sistema) : this()
    {
        _sistema = sistema;

        ComboUsuario.ItemsSource = sistema.Usuarios.Listar()
            .Where(u => u.Activo)
            .Select(u => new Opcion<Persona>(u, $"{u.Nombre}  ·  {u.TipoUsuario} ({u.EtiquetaDetalle}: {u.Detalle})"))
            .ToList();
        ComboLibro.ItemsSource = sistema.Libros.Listar()
            .Where(l => l.Disponible)
            .Select(l => new Opcion<Libro>(l, $"{l.Titulo}  ·  {l.Autor}  ({l.CopiasDisponibles} disp.)"))
            .ToList();
    }

    private Persona? Usuario => (ComboUsuario.SelectedItem as Opcion<Persona>)?.Valor;
    private Libro? Libro => (ComboLibro.SelectedItem as Opcion<Libro>)?.Valor;

    private void Seleccion_Cambio(object? sender, SelectionChangedEventArgs e)
    {
        if (_sistema is null)
            return;

        MensajeError.IsVisible = false;
        if (Usuario is not { } usuario)
        {
            Resumen.Text = "Seleccione el usuario y el libro para ver las condiciones del préstamo.";
            return;
        }

        var activos = _sistema.Prestamos.Listar(soloPendientes: true).Count(p => p.Usuario == usuario.Nombre);
        var pendiente = _sistema.Multas.TotalPendienteDe(usuario.Id);
        Resumen.Text =
            $"{usuario.TipoUsuario}: plazo de {usuario.DiasPrestamo} días, vence el {DateTime.Today.AddDays(usuario.DiasPrestamo):dd/MM/yyyy}.\n" +
            $"Préstamos activos: {activos} de {usuario.LimitePrestamos} permitidos." +
            (pendiente > 0 ? $"\nAtención: tiene multas pendientes por Q{pendiente:N2}." : string.Empty);
    }

    private void Guardar_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (Usuario is not { } usuario)
                throw new ValidacionException("Seleccione el usuario que solicita el préstamo.");
            if (Libro is not { } libro)
                throw new ValidacionException("Seleccione el libro que se va a prestar.");

            var prestamo = _sistema.Prestamos.Prestar(libro.Id, usuario.Id);
            MensajeExito =
                $"«{libro.Titulo}» fue prestado a {usuario.Nombre}.\n" +
                $"Fecha de devolución: {prestamo.FechaVencimiento:dd/MM/yyyy}.";
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
