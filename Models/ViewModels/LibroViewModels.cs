using System.ComponentModel.DataAnnotations;
using Biblioteca.Core.Modelos;

namespace Biblioteca.Web.Models.ViewModels;

/// <summary>Datos del formulario de alta y edicion de un libro.</summary>
public class LibroFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El ISBN es obligatorio.")]
    [Display(Name = "ISBN")]
    public string Isbn { get; set; } = string.Empty;

    [Required(ErrorMessage = "El título es obligatorio.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El autor es obligatorio.")]
    public string Autor { get; set; } = string.Empty;

    [Required(ErrorMessage = "La editorial es obligatoria.")]
    public string Editorial { get; set; } = string.Empty;

    [Required(ErrorMessage = "El año de publicación es obligatorio.")]
    [Display(Name = "Año de publicación")]
    public int? Anio { get; set; }

    [Display(Name = "Categoría")]
    public CategoriaLibro Categoria { get; set; } = CategoriaLibro.Otros;

    [Required(ErrorMessage = "La cantidad de ejemplares es obligatoria.")]
    [Display(Name = "Ejemplares")]
    public int? Copias { get; set; }

    /// <summary>Ejemplares en prestamo (solo informativo al editar).</summary>
    public int CopiasPrestadas { get; set; }
}

/// <summary>Listado de libros con los filtros aplicados.</summary>
public class LibrosIndexViewModel
{
    public IReadOnlyList<Libro> Libros { get; set; } = Array.Empty<Libro>();
    public string? Q { get; set; }
    public CategoriaLibro? Categoria { get; set; }
    public bool SoloDisponibles { get; set; }
}
