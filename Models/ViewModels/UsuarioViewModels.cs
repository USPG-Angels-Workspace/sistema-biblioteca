using System.ComponentModel.DataAnnotations;
using Biblioteca.Core.Modelos;

namespace Biblioteca.Web.Models.ViewModels;

/// <summary>Datos del formulario de alta y edicion de un usuario (lector o bibliotecario).</summary>
public class UsuarioFormViewModel
{
    public int Id { get; set; }

    [Required]
    public string Tipo { get; set; } = "Lector";

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El documento (DPI) es obligatorio.")]
    [Display(Name = "Documento (DPI)")]
    public string Documento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    /// <summary>Carnet (lector) o cargo (bibliotecario).</summary>
    [Required(ErrorMessage = "El carnet o cargo es obligatorio.")]
    [Display(Name = "Carnet / Cargo")]
    public string Detalle { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}

/// <summary>Listado de usuarios con los filtros aplicados.</summary>
public class UsuariosIndexViewModel
{
    public IReadOnlyList<Persona> Usuarios { get; set; } = Array.Empty<Persona>();
    public string? Q { get; set; }
    public string? Tipo { get; set; }
}
