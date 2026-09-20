using Biblioteca.Core;
using Biblioteca.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Web.Controllers;

/// <summary>Consulta y cobro de multas.</summary>
public class MultasController : BaseController
{
    private readonly SistemaBiblioteca _sistema;

    public MultasController(SistemaBiblioteca sistema) => _sistema = sistema;

    /// <summary>Lista las multas con filtro por texto y estado de pago.</summary>
    public IActionResult Index(string? q, string? estado)
    {
        bool? pagadas = estado switch { "pendientes" => false, "pagadas" => true, _ => null };
        return View(new MultasIndexViewModel
        {
            Multas = _sistema.Multas.Listar(pagadas, q),
            Q = q,
            Estado = estado
        });
    }

    /// <summary>Registra el pago de una multa.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Pagar(int id)
    {
        Biblioteca.Core.Modelos.Multa? multa = null;
        if (EjecutarConAviso(() => multa = _sistema.Multas.Pagar(id)))
            TempData["Mensaje"] = $"Se registró el pago de Q{multa!.Monto:N2}.";
        return RedirectToAction(nameof(Index), new { estado = "pendientes" });
    }
}
