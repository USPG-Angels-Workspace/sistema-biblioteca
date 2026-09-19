using Biblioteca.Core.Modelos;

namespace Biblioteca.Core.Servicios;

/// <summary>Vista de un préstamo con los nombres del libro y del usuario ya resueltos.</summary>
public record PrestamoDetalle(
    int Id,
    string Libro,
    string Usuario,
    DateTime FechaPrestamo,
    DateTime FechaVencimiento,
    DateTime? FechaDevolucion,
    int Renovaciones,
    EstadoPrestamo Estado,
    int DiasAtraso)
{
    public string EstadoTexto => Estado.ToString();
    public string FechaDevolucionTexto => FechaDevolucion?.ToString("dd/MM/yyyy") ?? "—";
    public string FechaPrestamoTexto => FechaPrestamo.ToString("dd/MM/yyyy");
    public string FechaVencimientoTexto => FechaVencimiento.ToString("dd/MM/yyyy");
}
