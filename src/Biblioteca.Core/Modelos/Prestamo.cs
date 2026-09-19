using System.Text.Json.Serialization;
using Biblioteca.Core.Excepciones;

namespace Biblioteca.Core.Modelos;

public class Prestamo : EntidadBase
{
    public const int MaximoRenovaciones = 2;

    [JsonInclude] public int LibroId { get; private set; }
    [JsonInclude] public int UsuarioId { get; private set; }
    [JsonInclude] public DateTime FechaPrestamo { get; private set; }
    [JsonInclude] public DateTime FechaVencimiento { get; private set; }
    [JsonInclude] public DateTime? FechaDevolucion { get; private set; }
    [JsonInclude] public int Renovaciones { get; private set; }

    [JsonIgnore] public bool Devuelto => FechaDevolucion.HasValue;

    [JsonConstructor]
    private Prestamo() { }

    public Prestamo(int libroId, int usuarioId, DateTime fechaPrestamo, int diasPrestamo)
    {
        if (diasPrestamo < 1)
            throw new ValidacionException("El plazo del préstamo debe ser de al menos un día.");
        LibroId = libroId;
        UsuarioId = usuarioId;
        FechaPrestamo = fechaPrestamo.Date;
        FechaVencimiento = FechaPrestamo.AddDays(diasPrestamo);
    }

    public EstadoPrestamo ObtenerEstado(DateTime hoy)
    {
        if (Devuelto)
            return EstadoPrestamo.Devuelto;
        return hoy.Date > FechaVencimiento.Date ? EstadoPrestamo.Atrasado : EstadoPrestamo.Activo;
    }

    /// <summary>Días transcurridos después del vencimiento; si ya se devolvió, se cuenta hasta la devolución.</summary>
    public int DiasAtraso(DateTime referencia)
    {
        var corte = (FechaDevolucion ?? referencia).Date;
        return Math.Max(0, (corte - FechaVencimiento.Date).Days);
    }

    public void Renovar(int dias, DateTime hoy)
    {
        if (Devuelto)
            throw new ValidacionException("El préstamo ya fue devuelto y no puede renovarse.");
        if (Renovaciones >= MaximoRenovaciones)
            throw new ValidacionException($"Se alcanzó el máximo de {MaximoRenovaciones} renovaciones por préstamo.");
        if (hoy.Date > FechaVencimiento.Date)
            throw new ValidacionException("El préstamo está vencido: debe devolverse, no renovarse.");
        if (dias < 1)
            throw new ValidacionException("El plazo de la renovación debe ser de al menos un día.");

        FechaVencimiento = FechaVencimiento.AddDays(dias);
        Renovaciones++;
    }

    public void RegistrarDevolucion(DateTime fecha)
    {
        if (Devuelto)
            throw new ValidacionException("Este préstamo ya fue devuelto.");
        if (fecha.Date < FechaPrestamo.Date)
            throw new ValidacionException("La fecha de devolución no puede ser anterior a la del préstamo.");
        FechaDevolucion = fecha.Date;
    }
}
