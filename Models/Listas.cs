using Biblioteca.Core.Modelos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Biblioteca.Web.Models;

/// <summary>Listas desplegables reutilizadas por varias vistas.</summary>
public static class Listas
{
    public static List<SelectListItem> Categorias(CategoriaLibro? seleccionada = null) =>
        Enum.GetValues<CategoriaLibro>()
            .Select(c => new SelectListItem(c.Texto(), c.ToString(), c == seleccionada))
            .ToList();
}
