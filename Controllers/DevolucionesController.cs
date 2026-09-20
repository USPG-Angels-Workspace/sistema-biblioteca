using Biblioteca.Core;
using Biblioteca.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Web.Controllers;

/// <summary>Registro de devoluciones y control de vencimientos.</summary>
public class DevolucionesController : BaseController
{
    private readonly SistemaBiblioteca _sistema;

    public DevolucionesController(SistemaBiblioteca sistema) => _sistema = sistema;

    /// <summary>Lista los prestamos sin devolver con sus dias de atraso y multa estimada.</summary>
    public IActionResult Index(string? q)
    {
        var filas = _sistema.Prestamos.Listar(soloPendientes: true, texto: q)
            .Select(p => new FilaDevolucion(p, _sistema.Multas.CalcularMonto(p.DiasAtraso)))
            .ToList();
        return View(new DevolucionesIndexViewModel { Filas = filas, Q = q });
    }

    /// <summary>Registra la devolucion; si hay atraso genera la multa.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Registrar(int id)
    {
        Biblioteca.Core.Servicios.ResultadoDevolucion? resultado = null;
        if (EjecutarConAviso(() => resultado = _sistema.Devoluciones.Registrar(id)))
        {
            var libro = _sistema.Libros.ObtenerPorId(resultado!.Prestamo.LibroId)?.Titulo;
            var mensaje = $"«{libro}» fue devuelto y el ejemplar quedó disponible.";
            if (resultado.Multa is { } multa)
                mensaje += $" Se generó una multa de Q{multa.Monto:N2} por {multa.DiasAtraso} día(s) de atraso; " +
                           "mientras no se pague, el usuario no podrá solicitar nuevos préstamos.";
            TempData["Mensaje"] = mensaje;
        }
        return RedirectToAction(nameof(Index));
    }
}
