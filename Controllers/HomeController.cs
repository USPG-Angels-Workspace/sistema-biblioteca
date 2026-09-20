using System.Diagnostics;
using Biblioteca.Core;
using Biblioteca.Core.Modelos;
using Biblioteca.Web.Models;
using Biblioteca.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Web.Controllers;

/// <summary>Panel principal con el resumen del estado de la biblioteca.</summary>
public class HomeController : Controller
{
    private readonly SistemaBiblioteca _sistema;

    public HomeController(SistemaBiblioteca sistema) => _sistema = sistema;

    /// <summary>Muestra los indicadores generales y los proximos vencimientos.</summary>
    public IActionResult Index()
    {
        var libros = _sistema.Libros.Listar();
        var usuarios = _sistema.Usuarios.Listar();
        var pendientes = _sistema.Prestamos.Listar(soloPendientes: true);

        return View(new DashboardViewModel
        {
            TotalLibros = libros.Count,
            TotalEjemplares = libros.Sum(l => l.CopiasTotales),
            EjemplaresDisponibles = libros.Sum(l => l.CopiasDisponibles),
            TotalUsuarios = usuarios.Count,
            UsuariosActivos = usuarios.Count(u => u.Activo),
            PrestamosPendientes = pendientes.Count,
            PrestamosAtrasados = pendientes.Count(p => p.Estado == EstadoPrestamo.Atrasado),
            MultasPendientes = _sistema.Multas.Listar(pagadas: false).Sum(m => m.Monto),
            ProximosVencimientos = pendientes.Take(5).ToList()
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
