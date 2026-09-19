namespace Biblioteca.Core.Servicios;

/// <summary>Vista de una multa con los nombres del usuario y del libro ya resueltos.</summary>
public record MultaDetalle(
    int Id,
    string Usuario,
    string Libro,
    int DiasAtraso,
    decimal Monto,
    DateTime FechaGeneracion,
    bool Pagada,
    DateTime? FechaPago)
{
    public string EstadoTexto => Pagada ? "Pagada" : "Pendiente";
    public string MontoTexto => $"Q{Monto.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)}";
    public string FechaGeneracionTexto => FechaGeneracion.ToString("dd/MM/yyyy");
    public string FechaPagoTexto => FechaPago?.ToString("dd/MM/yyyy") ?? "—";
}
