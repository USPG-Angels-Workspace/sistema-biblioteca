using Biblioteca.Core.Servicios;

namespace Biblioteca.Web.Models.ViewModels;

/// <summary>Resumen mostrado en el panel principal.</summary>
public class DashboardViewModel
{
    public int TotalLibros { get; set; }
    public int TotalEjemplares { get; set; }
    public int EjemplaresDisponibles { get; set; }
    public int TotalUsuarios { get; set; }
    public int UsuariosActivos { get; set; }
    public int PrestamosPendientes { get; set; }
    public int PrestamosAtrasados { get; set; }
    public decimal MultasPendientes { get; set; }
    public IReadOnlyList<PrestamoDetalle> ProximosVencimientos { get; set; } = Array.Empty<PrestamoDetalle>();
}

/// <summary>Reporte seleccionado mas la lista de reportes disponibles.</summary>
public class ReportesIndexViewModel
{
    public string Clave { get; set; } = "disponibles";
    public ReporteTabular Reporte { get; set; } = null!;
    public IReadOnlyList<(string Clave, string Nombre)> Opciones { get; set; } = Array.Empty<(string, string)>();
}
