using Biblioteca.Core;
using Biblioteca.Core.Modelos;
using Biblioteca.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Web.Controllers;

/// <summary>Registro de prestamos, asignacion de libros y renovaciones.</summary>
public class PrestamosController : BaseController
{
    private readonly SistemaBiblioteca _sistema;

    public PrestamosController(SistemaBiblioteca sistema) => _sistema = sistema;

    /// <summary>Lista los prestamos con filtro por texto y estado.</summary>
    public IActionResult Index(string? q, EstadoPrestamo? estado)
    {
        return View(new PrestamosIndexViewModel
        {
            Prestamos = _sistema.Prestamos.Listar(texto: q, estado: estado),
            Q = q,
            Estado = estado
        });
    }

    /// <summary>Muestra el formulario de nuevo prestamo.</summary>
    public IActionResult Nuevo() => View(CargarListas(new PrestamoNuevoViewModel()));

    /// <summary>Registra el prestamo aplicando todas las reglas del servicio.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Nuevo(PrestamoNuevoViewModel modelo)
    {
        if (!ModelState.IsValid) return View(CargarListas(modelo));

        Prestamo? prestamo = null;
        if (!Ejecutar(() => prestamo = _sistema.Prestamos.Prestar(modelo.LibroId!.Value, modelo.UsuarioId!.Value)))
            return View(CargarListas(modelo));

        var libro = _sistema.Libros.ObtenerPorId(prestamo!.LibroId)?.Titulo;
        var usuario = _sistema.Usuarios.ObtenerPorId(prestamo.UsuarioId)?.Nombre;
        TempData["Mensaje"] = $"«{libro}» fue prestado a {usuario}. Fecha de devolución: {prestamo.FechaVencimiento:dd/MM/yyyy}.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Renueva un prestamo vigente.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Renovar(int id)
    {
        Prestamo? prestamo = null;
        if (EjecutarConAviso(() => prestamo = _sistema.Prestamos.Renovar(id)))
            TempData["Mensaje"] = $"Préstamo renovado. Nueva fecha de devolución: {prestamo!.FechaVencimiento:dd/MM/yyyy} " +
                                  $"(renovación {prestamo.Renovaciones} de {Prestamo.MaximoRenovaciones}).";
        return RedirectToAction(nameof(Index));
    }

    private PrestamoNuevoViewModel CargarListas(PrestamoNuevoViewModel modelo)
    {
        var pendientes = _sistema.Prestamos.Listar(soloPendientes: true);
        modelo.Usuarios = _sistema.Usuarios.Listar().Where(u => u.Activo).ToList();
        modelo.Libros = _sistema.Libros.Listar().Where(l => l.Disponible).ToList();
        modelo.PrestamosActivos = modelo.Usuarios.ToDictionary(u => u.Id, u => pendientes.Count(p => p.Usuario == u.Nombre));
        modelo.MultasPendientes = modelo.Usuarios.ToDictionary(u => u.Id, u => _sistema.Multas.TotalPendienteDe(u.Id));
        return modelo;
    }
}
