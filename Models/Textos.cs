using Biblioteca.Core.Modelos;
using Biblioteca.Core.Servicios;

namespace Biblioteca.Web.Models;

/// <summary>Textos cortos para las vistas: plurales y plazos dichos como los diria una persona.</summary>
public static class Textos
{
    public static string Cantidad(int cantidad, string singular, string plural) =>
        $"{cantidad} {(cantidad == 1 ? singular : plural)}";

    /// <summary>"Vence hoy", "Vence en 3 días", "4 días de atraso" o "Devuelto el 15/09/2026".</summary>
    public static string Vencimiento(PrestamoDetalle prestamo)
    {
        if (prestamo.Estado == EstadoPrestamo.Devuelto) return $"Devuelto el {prestamo.FechaDevolucionTexto}";
        if (prestamo.Estado == EstadoPrestamo.Atrasado) return Cantidad(prestamo.DiasAtraso, "día de atraso", "días de atraso");

        var dias = (prestamo.FechaVencimiento.Date - DateTime.Today).Days;
        return dias switch
        {
            <= 0 => "Vence hoy",
            1 => "Vence mañana",
            _ => $"Vence en {dias} días"
        };
    }
}
