using System.ComponentModel.DataAnnotations;
using Biblioteca.Core.Modelos;
using Biblioteca.Core.Servicios;

namespace Biblioteca.Web.Models.ViewModels;

/// <summary>Listado de prestamos con los filtros aplicados.</summary>
public class PrestamosIndexViewModel
{
    public IReadOnlyList<PrestamoDetalle> Prestamos { get; set; } = Array.Empty<PrestamoDetalle>();
    public string? Q { get; set; }
    public EstadoPrestamo? Estado { get; set; }
}

/// <summary>Formulario de nuevo prestamo: usuario y libro a prestar.</summary>
public class PrestamoNuevoViewModel
{
    [Required(ErrorMessage = "Seleccione el usuario que solicita el préstamo.")]
    [Display(Name = "Usuario")]
    public int? UsuarioId { get; set; }

    [Required(ErrorMessage = "Seleccione el libro que se va a prestar.")]
    [Display(Name = "Libro")]
    public int? LibroId { get; set; }

    public IReadOnlyList<Persona> Usuarios { get; set; } = Array.Empty<Persona>();
    public IReadOnlyList<Libro> Libros { get; set; } = Array.Empty<Libro>();

    /// <summary>Prestamos sin devolver por usuario, para mostrar el resumen de condiciones.</summary>
    public Dictionary<int, int> PrestamosActivos { get; set; } = new();

    /// <summary>Total de multas pendientes por usuario.</summary>
    public Dictionary<int, decimal> MultasPendientes { get; set; } = new();
}

/// <summary>Fila de la pantalla de devoluciones: prestamo pendiente y su multa estimada.</summary>
public record FilaDevolucion(PrestamoDetalle Prestamo, decimal MultaEstimada);

/// <summary>Pantalla de devoluciones pendientes.</summary>
public class DevolucionesIndexViewModel
{
    public IReadOnlyList<FilaDevolucion> Filas { get; set; } = Array.Empty<FilaDevolucion>();
    public string? Q { get; set; }
}

/// <summary>Listado de multas con los filtros aplicados.</summary>
public class MultasIndexViewModel
{
    public IReadOnlyList<MultaDetalle> Multas { get; set; } = Array.Empty<MultaDetalle>();
    public string? Q { get; set; }

    /// <summary>"pendientes", "pagadas" o vacio para todas.</summary>
    public string? Estado { get; set; }
}
