using System.Globalization;
using Biblioteca.Core.Interfaces;
using Biblioteca.Core.Modelos;

namespace Biblioteca.Core.Servicios;

public class ReporteService
{
    private readonly IRepositorio<Libro> _libros;
    private readonly IRepositorio<Persona> _personas;
    private readonly IRepositorio<Prestamo> _prestamos;
    private readonly IRepositorio<Multa> _multas;
    private readonly ICalculadoraMulta _calculadora;
    private readonly IReloj _reloj;

    public ReporteService(IRepositorio<Libro> libros, IRepositorio<Persona> personas,
                          IRepositorio<Prestamo> prestamos, IRepositorio<Multa> multas,
                          ICalculadoraMulta calculadora, IReloj reloj)
    {
        _libros = libros;
        _personas = personas;
        _prestamos = prestamos;
        _multas = multas;
        _calculadora = calculadora;
        _reloj = reloj;
    }

    public ReporteTabular LibrosDisponibles()
    {
        var libros = _libros.ObtenerTodos().Where(l => l.Disponible)
            .OrderBy(l => l.Titulo, StringComparer.CurrentCultureIgnoreCase).ToList();
        var filas = libros.Select(l => Fila(l.Isbn, l.Titulo, l.Autor, l.CategoriaTexto,
                                            $"{l.CopiasDisponibles} de {l.CopiasTotales}")).ToList();
        return new ReporteTabular("Libros disponibles",
            new[] { "ISBN", "Título", "Autor", "Categoría", "Ejemplares disponibles" },
            filas,
            $"{libros.Count} títulos con ejemplares disponibles ({libros.Sum(l => l.CopiasDisponibles)} ejemplares).");
    }

    public ReporteTabular PrestamosActivos()
    {
        var hoy = _reloj.Hoy;
        var libros = _libros.ObtenerTodos().ToDictionary(l => l.Id);
        var personas = _personas.ObtenerTodos().ToDictionary(p => p.Id);
        var activos = _prestamos.ObtenerTodos().Where(p => !p.Devuelto).OrderBy(p => p.FechaVencimiento).ToList();
        var filas = activos.Select(p => Fila(
            NombreLibro(libros, p.LibroId), NombreUsuario(personas, p.UsuarioId),
            Fecha(p.FechaPrestamo), Fecha(p.FechaVencimiento),
            p.Renovaciones.ToString(CultureInfo.InvariantCulture), p.ObtenerEstado(hoy).ToString())).ToList();
        return new ReporteTabular("Préstamos activos",
            new[] { "Libro", "Usuario", "Fecha de préstamo", "Vence", "Renovaciones", "Estado" },
            filas, $"{activos.Count} préstamos sin devolver.");
    }

    public ReporteTabular LibrosAtrasados()
    {
        var hoy = _reloj.Hoy;
        var libros = _libros.ObtenerTodos().ToDictionary(l => l.Id);
        var personas = _personas.ObtenerTodos().ToDictionary(p => p.Id);
        var atrasados = _prestamos.ObtenerTodos()
            .Where(p => p.ObtenerEstado(hoy) == EstadoPrestamo.Atrasado)
            .OrderByDescending(p => p.DiasAtraso(hoy)).ToList();
        var filas = atrasados.Select(p =>
        {
            var dias = p.DiasAtraso(hoy);
            return Fila(NombreLibro(libros, p.LibroId), NombreUsuario(personas, p.UsuarioId),
                        Fecha(p.FechaVencimiento), dias.ToString(CultureInfo.InvariantCulture),
                        Dinero(_calculadora.Calcular(dias)));
        }).ToList();
        var total = atrasados.Sum(p => _calculadora.Calcular(p.DiasAtraso(hoy)));
        return new ReporteTabular("Libros atrasados",
            new[] { "Libro", "Usuario", "Venció el", "Días de atraso", "Multa estimada" },
            filas, $"{atrasados.Count} libros atrasados. Multas estimadas acumuladas: {Dinero(total)}.");
    }

    public ReporteTabular Usuarios()
    {
        var prestamos = _prestamos.ObtenerTodos();
        var multas = _multas.ObtenerTodos();
        var personas = _personas.ObtenerTodos()
            .OrderBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase).ToList();
        var filas = personas.Select(p => Fila(
            p.TipoUsuario, p.Nombre, p.Documento, $"{p.EtiquetaDetalle}: {p.Detalle}",
            p.Activo ? "Activo" : "Inactivo",
            prestamos.Count(x => x.UsuarioId == p.Id && !x.Devuelto).ToString(CultureInfo.InvariantCulture),
            Dinero(multas.Where(m => m.UsuarioId == p.Id && !m.Pagada).Sum(m => m.Monto)))).ToList();
        return new ReporteTabular("Usuarios",
            new[] { "Tipo", "Nombre", "Documento", "Carnet / Cargo", "Estado", "Préstamos activos", "Multas pendientes" },
            filas,
            $"{personas.Count(p => p is Lector)} lectores y {personas.Count(p => p is Bibliotecario)} bibliotecarios registrados.");
    }

    public ReporteTabular Multas()
    {
        var libros = _libros.ObtenerTodos().ToDictionary(l => l.Id);
        var personas = _personas.ObtenerTodos().ToDictionary(p => p.Id);
        var prestamos = _prestamos.ObtenerTodos().ToDictionary(p => p.Id);
        var multas = _multas.ObtenerTodos().OrderBy(m => m.Pagada).ThenByDescending(m => m.FechaGeneracion).ToList();
        var filas = multas.Select(m => Fila(
            NombreUsuario(personas, m.UsuarioId),
            prestamos.TryGetValue(m.PrestamoId, out var pr) ? NombreLibro(libros, pr.LibroId) : "(libro eliminado)",
            m.DiasAtraso.ToString(CultureInfo.InvariantCulture), Dinero(m.Monto),
            m.Pagada ? "Pagada" : "Pendiente", m.FechaPago is null ? "—" : Fecha(m.FechaPago.Value))).ToList();
        var pendiente = multas.Where(m => !m.Pagada).Sum(m => m.Monto);
        var cobrado = multas.Where(m => m.Pagada).Sum(m => m.Monto);
        return new ReporteTabular("Multas",
            new[] { "Usuario", "Libro", "Días de atraso", "Monto", "Estado", "Fecha de pago" },
            filas, $"Pendiente de cobro: {Dinero(pendiente)}. Cobrado: {Dinero(cobrado)}.");
    }

    private static IReadOnlyList<string> Fila(params string[] valores) => valores;

    private static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    private static string Dinero(decimal monto) => "Q" + monto.ToString("N2", CultureInfo.InvariantCulture);

    private static string NombreLibro(Dictionary<int, Libro> libros, int id) =>
        libros.TryGetValue(id, out var l) ? l.Titulo : "(libro eliminado)";

    private static string NombreUsuario(Dictionary<int, Persona> personas, int id) =>
        personas.TryGetValue(id, out var p) ? p.Nombre : "(usuario eliminado)";
}
