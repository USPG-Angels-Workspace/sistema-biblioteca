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

    // Transitorio: se retira cuando todas las vistas usen los estados nuevos.

    public static string InsigniaPrestamo(EstadoPrestamo estado) => estado switch
    {
        EstadoPrestamo.Atrasado => "bg-red-100 text-red-700 ring-red-200",
        EstadoPrestamo.Activo => "bg-indigo-100 text-indigo-700 ring-indigo-200",
        _ => "bg-slate-100 text-slate-600 ring-slate-200"
    };

    public static string InsigniaDisponible(bool disponible) =>
        disponible ? "bg-emerald-100 text-emerald-700 ring-emerald-200" : "bg-slate-100 text-slate-600 ring-slate-200";

    public static string InsigniaMulta(bool pagada) =>
        pagada ? "bg-slate-100 text-slate-600 ring-slate-200" : "bg-amber-100 text-amber-700 ring-amber-200";

    public static string FilaPrestamo(EstadoPrestamo estado) => estado switch
    {
        EstadoPrestamo.Atrasado => "bg-red-50/60",
        EstadoPrestamo.Devuelto => "text-slate-400",
        _ => ""
    };
}
