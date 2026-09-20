using Biblioteca.Core.Excepciones;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Web.Controllers;

/// <summary>
/// Base de los controladores: traduce las excepciones de negocio (ValidacionException)
/// y de archivos (AlmacenamientoException) en mensajes para el usuario.
/// </summary>
public abstract class BaseController : Controller
{
    /// <summary>Ejecuta la accion; si falla una regla, agrega el mensaje al ModelState y devuelve false.</summary>
    protected bool Ejecutar(Action accion)
    {
        try
        {
            accion();
            return true;
        }
        catch (Exception ex) when (ex is ValidacionException or AlmacenamientoException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return false;
        }
    }

    /// <summary>Ejecuta una accion sin formulario: en caso de error deja el mensaje en TempData["Error"].</summary>
    protected bool EjecutarConAviso(Action accion)
    {
        try
        {
            accion();
            return true;
        }
        catch (Exception ex) when (ex is ValidacionException or AlmacenamientoException)
        {
            TempData["Error"] = ex.Message;
            return false;
        }
    }
}
