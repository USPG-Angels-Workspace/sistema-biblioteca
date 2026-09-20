using System.Text;
using Biblioteca.Core;
using Biblioteca.Core.Servicios;
using Biblioteca.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Web.Controllers;

/// <summary>Reportes de libros, prestamos, atrasos, usuarios y multas, con exportacion a CSV.</summary>
public class ReportesController : Controller
{
    private static readonly (string Clave, string Nombre)[] Opciones =
    {
        ("disponibles", "Libros disponibles"),
        ("activos", "Préstamos activos"),
        ("atrasados", "Libros atrasados"),
        ("usuarios", "Usuarios"),
        ("multas", "Multas")
    };

    private readonly SistemaBiblioteca _sistema;

    public ReportesController(SistemaBiblioteca sistema) => _sistema = sistema;

    private ReporteTabular Generar(string clave) => clave switch
    {
        "activos" => _sistema.Reportes.PrestamosActivos(),
        "atrasados" => _sistema.Reportes.LibrosAtrasados(),
        "usuarios" => _sistema.Reportes.Usuarios(),
        "multas" => _sistema.Reportes.Multas(),
        _ => _sistema.Reportes.LibrosDisponibles()
    };

    /// <summary>Muestra el reporte elegido (por defecto, libros disponibles).</summary>
    public IActionResult Index(string? id)
    {
        var clave = Opciones.Any(o => o.Clave == id) ? id! : "disponibles";
        return View(new ReportesIndexViewModel { Clave = clave, Reporte = Generar(clave), Opciones = Opciones });
    }

    /// <summary>Descarga el reporte como archivo CSV (UTF-8 con BOM para abrirlo bien en Excel).</summary>
    public IActionResult Exportar(string? id)
    {
        var clave = Opciones.Any(o => o.Clave == id) ? id! : "disponibles";
        var reporte = Generar(clave);
        var bytes = new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(reporte.ACsv())).ToArray();
        return File(bytes, "text/csv", $"reporte-{clave}.csv");
    }
}
