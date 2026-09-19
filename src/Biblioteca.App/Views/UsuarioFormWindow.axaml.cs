using Avalonia.Controls;
using Avalonia.Interactivity;
using Biblioteca.App.Servicios;
using Biblioteca.Core;
using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Modelos;

namespace Biblioteca.App.Views;

public partial class UsuarioFormWindow : Window
{
    private const string TipoLector = "Lector";
    private const string TipoBibliotecario = "Bibliotecario";

    private readonly SistemaBiblioteca _sistema;
    private readonly Persona? _usuario;

    public string MensajeExito { get; private set; } = string.Empty;

    public UsuarioFormWindow()
    {
        InitializeComponent();
        _sistema = null!;
    }

    public UsuarioFormWindow(SistemaBiblioteca sistema, Persona? usuario = null) : this()
    {
        _sistema = sistema;
        _usuario = usuario;

        ComboTipo.ItemsSource = new List<Opcion<string>> { new(TipoLector, "Lector"), new(TipoBibliotecario, "Bibliotecario") };
        ComboTipo.SelectedIndex = 0;
        Title = Encabezado.Text = usuario is null ? "Nuevo usuario" : "Editar usuario";

        if (usuario is not null)
        {
            ComboTipo.SelectedIndex = usuario is Lector ? 0 : 1;
            ComboTipo.IsEnabled = false;
            CajaNombre.Text = usuario.Nombre;
            CajaDocumento.Text = usuario.Documento;
            CajaTelefono.Text = usuario.Telefono;
            CajaCorreo.Text = usuario.Correo;
            CajaDetalle.Text = usuario.Detalle;
            CheckActivo.IsChecked = usuario.Activo;
            CheckActivo.IsVisible = true;
        }

        ActualizarTipo();
    }

    private string TipoSeleccionado => (ComboTipo.SelectedItem as Opcion<string>)?.Valor ?? TipoLector;

    private void Tipo_Cambio(object? sender, SelectionChangedEventArgs e) => ActualizarTipo();

    private void ActualizarTipo()
    {
        if (Reglas is null || _sistema is null)
            return;

        var esLector = TipoSeleccionado == TipoLector;
        EtiquetaDetalle.Text = esLector ? "Carnet *" : "Cargo *";
        Reglas.Text = esLector
            ? "Lector: hasta 3 libros a la vez, con 7 días de préstamo."
            : "Bibliotecario: hasta 5 libros a la vez, con 14 días de préstamo.";
    }

    private void Guardar_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var nombre = CajaNombre.Text ?? "";
            var documento = CajaDocumento.Text ?? "";
            var correo = CajaCorreo.Text ?? "";
            var telefono = CajaTelefono.Text ?? "";
            var detalle = CajaDetalle.Text ?? "";

            Persona guardado;
            if (_usuario is null)
            {
                guardado = TipoSeleccionado == TipoLector
                    ? _sistema.Usuarios.RegistrarLector(nombre, documento, correo, telefono, detalle)
                    : _sistema.Usuarios.RegistrarBibliotecario(nombre, documento, correo, telefono, detalle);
                MensajeExito = $"{guardado.TipoUsuario} «{guardado.Nombre}» registrado correctamente.";
            }
            else
            {
                guardado = _sistema.Usuarios.Editar(_usuario.Id, nombre, documento, correo, telefono, detalle,
                                                    CheckActivo.IsChecked == true);
                MensajeExito = $"Los datos de «{guardado.Nombre}» se actualizaron correctamente.";
            }

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
