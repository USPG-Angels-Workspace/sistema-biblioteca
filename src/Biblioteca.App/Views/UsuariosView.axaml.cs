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

public partial class UsuariosView : UserControl
{
    private readonly SistemaBiblioteca _sistema;
    private readonly bool _listo;

    public UsuariosView(SistemaBiblioteca sistema)
    {
        InitializeComponent();
        _sistema = sistema;

        ComboTipo.ItemsSource = new List<Opcion<string?>>
        {
            new(null, "Todos los tipos"),
            new("Lector", "Lectores"),
            new("Bibliotecario", "Bibliotecarios")
        };
        ComboTipo.SelectedIndex = 0;

        _listo = true;
        Recargar();
    }

    private Window Ventana => (Window)TopLevel.GetTopLevel(this)!;
    private Persona? Seleccionado => Tabla.SelectedItem as Persona;

    private void Recargar()
    {
        if (!_listo)
            return;

        var tipo = (ComboTipo.SelectedItem as Opcion<string?>)?.Valor;
        var usuarios = _sistema.Usuarios.Buscar(CajaBusqueda.Text, tipo);
        Tabla.ItemsSource = usuarios;
        Contador.Text = usuarios.Count == 1 ? "1 usuario" : $"{usuarios.Count} usuarios";
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
        var formulario = new UsuarioFormWindow(_sistema);
        if (await formulario.ShowDialog<bool>(Ventana))
        {
            Recargar();
            await Dialogos.Informacion(Ventana, formulario.MensajeExito, "Usuario registrado");
        }
    }

    private async void Editar_Click(object? sender, RoutedEventArgs e) => await EditarSeleccionado();

    private async Task EditarSeleccionado()
    {
        if (Seleccionado is not { } usuario)
            return;

        var formulario = new UsuarioFormWindow(_sistema, usuario);
        if (await formulario.ShowDialog<bool>(Ventana))
        {
            Recargar();
            await Dialogos.Informacion(Ventana, formulario.MensajeExito, "Usuario actualizado");
        }
    }

    private async void Eliminar_Click(object? sender, RoutedEventArgs e)
    {
        if (Seleccionado is not { } usuario)
            return;

        if (!await Dialogos.Confirmar(Ventana, $"¿Eliminar a «{usuario.Nombre}»?\nEsta acción no se puede deshacer.", "Eliminar usuario"))
            return;

        try
        {
            _sistema.Usuarios.Eliminar(usuario.Id);
            Recargar();
            await Dialogos.Informacion(Ventana, $"«{usuario.Nombre}» fue eliminado del sistema.", "Usuario eliminado");
        }
        catch (Exception ex) when (ex is ValidacionException or AlmacenamientoException)
        {
            await Dialogos.Error(Ventana, ex.Message);
        }
    }
}
