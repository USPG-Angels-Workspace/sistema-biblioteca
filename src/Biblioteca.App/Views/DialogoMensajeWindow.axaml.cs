using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Biblioteca.App.Views;

public enum TipoDialogo
{
    Informacion,
    Error,
    Confirmacion
}

public partial class DialogoMensajeWindow : Window
{
    public DialogoMensajeWindow()
    {
        InitializeComponent();
    }

    public DialogoMensajeWindow(string titulo, string mensaje, TipoDialogo tipo, bool conCancelar) : this()
    {
        Title = titulo;
        Encabezado.Text = titulo;
        Mensaje.Text = mensaje;
        BotonCancelar.IsVisible = conCancelar;
        BotonAceptar.Content = tipo == TipoDialogo.Confirmacion ? "Sí, continuar" : "Aceptar";
        if (conCancelar)
            BotonCancelar.Content = "No, cancelar";

        (Simbolo.Text, Icono.Background) = tipo switch
        {
            TipoDialogo.Error => ("!", new SolidColorBrush(Color.Parse("#B3261E"))),
            TipoDialogo.Confirmacion => ("?", new SolidColorBrush(Color.Parse("#B26A00"))),
            _ => ("i", new SolidColorBrush(Color.Parse("#1F6FB2")))
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
            Close(false);
    }

    private void Aceptar_Click(object? sender, RoutedEventArgs e) => Close(true);

    private void Cancelar_Click(object? sender, RoutedEventArgs e) => Close(false);
}
