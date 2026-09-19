using Biblioteca.Core.Modelos;

namespace Biblioteca.Core.Servicios;

/// <summary>Resultado de registrar una devolución: el préstamo cerrado y la multa generada, si hubo atraso.</summary>
public record ResultadoDevolucion(Prestamo Prestamo, Multa? Multa);
