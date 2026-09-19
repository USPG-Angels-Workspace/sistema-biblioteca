using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Interfaces;
using Biblioteca.Core.Modelos;

namespace Biblioteca.Core.Servicios;

public class PrestamoService
{
    private readonly IRepositorio<Libro> _libros;
    private readonly IRepositorio<Persona> _personas;
    private readonly IRepositorio<Prestamo> _prestamos;
    private readonly IRepositorio<Multa> _multas;
    private readonly IReloj _reloj;

    public PrestamoService(IRepositorio<Libro> libros, IRepositorio<Persona> personas,
                           IRepositorio<Prestamo> prestamos, IRepositorio<Multa> multas, IReloj reloj)
    {
        _libros = libros;
        _personas = personas;
        _prestamos = prestamos;
        _multas = multas;
        _reloj = reloj;
    }

    public Prestamo Prestar(int libroId, int usuarioId)
    {
        var libro = _libros.ObtenerPorId(libroId)
            ?? throw new ValidacionException("Debe seleccionar un libro válido.");
        var usuario = _personas.ObtenerPorId(usuarioId)
            ?? throw new ValidacionException("Debe seleccionar un usuario válido.");

        if (!usuario.Activo)
            throw new ValidacionException($"«{usuario.Nombre}» está inactivo y no puede solicitar préstamos.");
        if (_multas.ObtenerTodos().Any(m => m.UsuarioId == usuarioId && !m.Pagada))
            throw new ValidacionException($"«{usuario.Nombre}» tiene multas pendientes de pago.");

        var activos = _prestamos.ObtenerTodos().Where(p => p.UsuarioId == usuarioId && !p.Devuelto).ToList();
        if (activos.Any(p => p.ObtenerEstado(_reloj.Hoy) == EstadoPrestamo.Atrasado))
            throw new ValidacionException($"«{usuario.Nombre}» tiene préstamos atrasados por devolver.");
        if (activos.Count >= usuario.LimitePrestamos)
            throw new ValidacionException(
                $"«{usuario.Nombre}» alcanzó su límite de {usuario.LimitePrestamos} préstamos simultáneos.");
        if (activos.Any(p => p.LibroId == libroId))
            throw new ValidacionException($"«{usuario.Nombre}» ya tiene un ejemplar de «{libro.Titulo}» en préstamo.");
        if (!libro.Disponible)
            throw new ValidacionException($"No hay ejemplares disponibles de «{libro.Titulo}».");

        var prestamo = new Prestamo(libroId, usuarioId, _reloj.Hoy, usuario.DiasPrestamo);
        libro.PrestarCopia();
        _prestamos.Agregar(prestamo);
        _libros.Actualizar(libro);
        return prestamo;
    }

    public Prestamo Renovar(int prestamoId)
    {
        var prestamo = _prestamos.ObtenerPorId(prestamoId)
            ?? throw new ValidacionException("El préstamo seleccionado ya no existe.");
        var usuario = _personas.ObtenerPorId(prestamo.UsuarioId)
            ?? throw new ValidacionException("El usuario del préstamo ya no existe.");

        prestamo.Renovar(usuario.DiasPrestamo, _reloj.Hoy);
        _prestamos.Actualizar(prestamo);
        return prestamo;
    }

    public Prestamo? ObtenerPorId(int id) => _prestamos.ObtenerPorId(id);

    public IReadOnlyList<PrestamoDetalle> Listar(bool soloPendientes = false, string? texto = null, EstadoPrestamo? estado = null)
    {
        var hoy = _reloj.Hoy;
        var libros = _libros.ObtenerTodos().ToDictionary(l => l.Id);
        var personas = _personas.ObtenerTodos().ToDictionary(p => p.Id);

        var detalle = _prestamos.ObtenerTodos()
            .Where(p => !soloPendientes || !p.Devuelto)
            .Where(p => estado is null || p.ObtenerEstado(hoy) == estado)
            .Select(p => new PrestamoDetalle(
                p.Id,
                libros.TryGetValue(p.LibroId, out var l) ? l.Titulo : "(libro eliminado)",
                personas.TryGetValue(p.UsuarioId, out var u) ? u.Nombre : "(usuario eliminado)",
                p.FechaPrestamo, p.FechaVencimiento, p.FechaDevolucion, p.Renovaciones,
                p.ObtenerEstado(hoy), p.DiasAtraso(hoy)));

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var buscado = texto.Trim();
            detalle = detalle.Where(d =>
                d.Libro.Contains(buscado, StringComparison.CurrentCultureIgnoreCase) ||
                d.Usuario.Contains(buscado, StringComparison.CurrentCultureIgnoreCase));
        }

        return detalle.OrderBy(d => d.Estado == EstadoPrestamo.Devuelto)
                      .ThenBy(d => d.FechaVencimiento)
                      .ToList();
    }
}
