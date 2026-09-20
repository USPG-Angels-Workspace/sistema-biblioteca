using Biblioteca.Core.Modelos;

namespace Biblioteca.Web.Models;

/// <summary>
/// Clases del indicador de estado (un punto de color junto al texto, definido en _Layout),
/// para no repetir la logica en cada vista.
/// </summary>
public static class Estilos
{
    public static string EstadoDePrestamo(EstadoPrestamo estado) => estado switch
    {
        EstadoPrestamo.Atrasado => "estado estado-rojo",
        EstadoPrestamo.Activo => "estado estado-verde",
        _ => "estado estado-gris"
    };

    public static string EstadoDeUsuario(bool activo) => activo ? "estado estado-verde" : "estado estado-gris";

    public static string EstadoDeMulta(bool pagada) => pagada ? "estado estado-gris" : "estado estado-ambar";

    /// <summary>Un prestamo ya devuelto se atenua para que no compita con los pendientes.</summary>
    public static string FilaPrestamo(EstadoPrestamo estado) =>
        estado == EstadoPrestamo.Devuelto ? "text-suave" : "";
}
