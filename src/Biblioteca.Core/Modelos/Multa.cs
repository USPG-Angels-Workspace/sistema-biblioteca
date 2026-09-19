using System.Text.Json.Serialization;
using Biblioteca.Core.Excepciones;

namespace Biblioteca.Core.Modelos;

public class Multa : EntidadBase
{
    [JsonInclude] public int PrestamoId { get; private set; }
    [JsonInclude] public int UsuarioId { get; private set; }
    [JsonInclude] public int DiasAtraso { get; private set; }
    [JsonInclude] public decimal Monto { get; private set; }
    [JsonInclude] public DateTime FechaGeneracion { get; private set; }
    [JsonInclude] public bool Pagada { get; private set; }
    [JsonInclude] public DateTime? FechaPago { get; private set; }

    [JsonConstructor]
    private Multa() { }

    public Multa(int prestamoId, int usuarioId, int diasAtraso, decimal monto, DateTime fechaGeneracion)
    {
        if (diasAtraso < 1)
            throw new ValidacionException("Una multa requiere al menos un día de atraso.");
        if (monto <= 0)
            throw new ValidacionException("El monto de la multa debe ser mayor que cero.");
        PrestamoId = prestamoId;
        UsuarioId = usuarioId;
        DiasAtraso = diasAtraso;
        Monto = monto;
        FechaGeneracion = fechaGeneracion.Date;
    }

    public void Pagar(DateTime fecha)
    {
        if (Pagada)
            throw new ValidacionException("La multa ya fue pagada.");
        Pagada = true;
        FechaPago = fecha.Date;
    }
}
