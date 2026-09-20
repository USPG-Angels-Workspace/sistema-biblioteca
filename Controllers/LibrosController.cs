using Biblioteca.Core;
using Biblioteca.Core.Modelos;
using Biblioteca.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Web.Controllers;

/// <summary>Alta, edicion, eliminacion, busqueda y clasificacion de libros.</summary>
public class LibrosController : BaseController
{
    private readonly SistemaBiblioteca _sistema;

    public LibrosController(SistemaBiblioteca sistema) => _sistema = sistema;

    /// <summary>Lista los libros con filtro por texto, categoria y disponibilidad.</summary>
    public IActionResult Index(string? q, CategoriaLibro? categoria, bool soloDisponibles)
    {
        return View(new LibrosIndexViewModel
        {
            Libros = _sistema.Libros.Buscar(q, categoria, soloDisponibles),
            Q = q,
            Categoria = categoria,
            SoloDisponibles = soloDisponibles
        });
    }

    /// <summary>Muestra el formulario para registrar un libro.</summary>
    public IActionResult Crear() => View(new LibroFormViewModel());

    /// <summary>Registra un libro nuevo.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(LibroFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        Libro? libro = null;
        if (!Ejecutar(() => libro = _sistema.Libros.Registrar(modelo.Isbn, modelo.Titulo, modelo.Autor,
                modelo.Editorial, modelo.Anio!.Value, modelo.Categoria, modelo.Copias!.Value)))
            return View(modelo);

        TempData["Mensaje"] = $"El libro «{libro!.Titulo}» se registró correctamente con {libro.CopiasTotales} ejemplar(es).";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Muestra el formulario de edicion de un libro.</summary>
    public IActionResult Editar(int id)
    {
        var libro = _sistema.Libros.ObtenerPorId(id);
        if (libro is null) return NotFound();

        return View(new LibroFormViewModel
        {
            Id = libro.Id,
            Isbn = libro.Isbn,
            Titulo = libro.Titulo,
            Autor = libro.Autor,
            Editorial = libro.Editorial,
            Anio = libro.Anio,
            Categoria = libro.Categoria,
            Copias = libro.CopiasTotales,
            CopiasPrestadas = libro.CopiasPrestadas
        });
    }

    /// <summary>Guarda los cambios de un libro.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(LibroFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        Libro? libro = null;
        if (!Ejecutar(() => libro = _sistema.Libros.Editar(modelo.Id, modelo.Isbn, modelo.Titulo, modelo.Autor,
                modelo.Editorial, modelo.Anio!.Value, modelo.Categoria, modelo.Copias!.Value)))
            return View(modelo);

        TempData["Mensaje"] = $"Los datos de «{libro!.Titulo}» se actualizaron correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Elimina un libro que no tenga historial de prestamos.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        var titulo = _sistema.Libros.ObtenerPorId(id)?.Titulo ?? "el libro";
        if (EjecutarConAviso(() => _sistema.Libros.Eliminar(id)))
            TempData["Mensaje"] = $"El libro «{titulo}» fue eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
