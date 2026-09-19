using Avalonia.Controls;
using Biblioteca.App.Views;

namespace Biblioteca.App.Servicios;

/// <summary>Mensajes de información, error y confirmación para el usuario.</summary>
public static class Dialogos
{
    public static Task Informacion(Window propietario, string mensaje, string titulo = "Información") =>
        new DialogoMensajeWindow(titulo, mensaje, TipoDialogo.Informacion, conCancelar: false)
            .ShowDialog<bool>(propietario);

    public static Task Error(Window propietario, string mensaje, string titulo = "No se pudo completar la operación") =>
        new DialogoMensajeWindow(titulo, mensaje, TipoDialogo.Error, conCancelar: false)
            .ShowDialog<bool>(propietario);

    public static Task<bool> Confirmar(Window propietario, string mensaje, string titulo = "Confirmar") =>
        new DialogoMensajeWindow(titulo, mensaje, TipoDialogo.Confirmacion, conCancelar: true)
            .ShowDialog<bool>(propietario);
}
