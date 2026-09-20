using Biblioteca.Core;
using Biblioteca.Core.Modelos;
using Biblioteca.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Web.Controllers;

/// <summary>Registro, edicion, desactivacion y consulta de lectores y bibliotecarios.</summary>
public class UsuariosController : BaseController
{
    private readonly SistemaBiblioteca _sistema;

    public UsuariosController(SistemaBiblioteca sistema) => _sistema = sistema;

    /// <summary>Lista los usuarios con filtro por texto y tipo.</summary>
    public IActionResult Index(string? q, string? tipo)
    {
        return View(new UsuariosIndexViewModel
        {
            Usuarios = _sistema.Usuarios.Buscar(q, tipo),
            Q = q,
            Tipo = tipo
        });
    }

    /// <summary>Muestra el formulario para registrar un usuario.</summary>
    public IActionResult Crear() => View(new UsuarioFormViewModel());

    /// <summary>Registra un lector o un bibliotecario segun el tipo elegido.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(UsuarioFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        Persona? usuario = null;
        var registrado = Ejecutar(() => usuario = modelo.Tipo == "Bibliotecario"
            ? _sistema.Usuarios.RegistrarBibliotecario(modelo.Nombre, modelo.Documento, modelo.Correo, modelo.Telefono, modelo.Detalle)
            : _sistema.Usuarios.RegistrarLector(modelo.Nombre, modelo.Documento, modelo.Correo, modelo.Telefono, modelo.Detalle));
        if (!registrado) return View(modelo);

        TempData["Mensaje"] = $"{usuario!.TipoUsuario} «{usuario.Nombre}» registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Muestra el formulario de edicion de un usuario.</summary>
    public IActionResult Editar(int id)
    {
        var usuario = _sistema.Usuarios.ObtenerPorId(id);
        if (usuario is null) return NotFound();

        return View(new UsuarioFormViewModel
        {
            Id = usuario.Id,
            Tipo = usuario.TipoUsuario,
            Nombre = usuario.Nombre,
            Documento = usuario.Documento,
            Correo = usuario.Correo,
            Telefono = usuario.Telefono,
            Detalle = usuario.Detalle,
            Activo = usuario.Activo
        });
    }

    /// <summary>Guarda los cambios de un usuario.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(UsuarioFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        Persona? usuario = null;
        if (!Ejecutar(() => usuario = _sistema.Usuarios.Editar(modelo.Id, modelo.Nombre, modelo.Documento,
                modelo.Correo, modelo.Telefono, modelo.Detalle, modelo.Activo)))
            return View(modelo);

        TempData["Mensaje"] = $"Los datos de «{usuario!.Nombre}» se actualizaron correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Elimina un usuario sin prestamos ni multas en su historial.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        var nombre = _sistema.Usuarios.ObtenerPorId(id)?.Nombre ?? "el usuario";
        if (EjecutarConAviso(() => _sistema.Usuarios.Eliminar(id)))
            TempData["Mensaje"] = $"«{nombre}» fue eliminado del sistema.";
        return RedirectToAction(nameof(Index));
    }
}
