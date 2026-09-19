using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Interfaces;
using Biblioteca.Core.Modelos;

namespace Biblioteca.Core.Servicios;

public class MultaService
{
    private readonly IRepositorio<Multa> _multas;
    private readonly IRepositorio<Prestamo> _prestamos;
    private readonly IRepositorio<Persona> _personas;
    private readonly IRepositorio<Libro> _libros;
    private readonly ICalculadoraMulta _calculadora;
    private readonly IReloj _reloj;

    public MultaService(IRepositorio<Multa> multas, IRepositorio<Prestamo> prestamos,
                        IRepositorio<Persona> personas, IRepositorio<Libro> libros,
                        ICalculadoraMulta calculadora, IReloj reloj)
    {
        _multas = multas;
        _prestamos = prestamos;
        _personas = personas;
        _libros = libros;
        _calculadora = calculadora;
        _reloj = reloj;
    }

    public decimal CalcularMonto(int diasAtraso) => _calculadora.Calcular(diasAtraso);

    /// <summary>Genera la multa de un préstamo ya devuelto; devuelve null si se entregó a tiempo.</summary>
    public Multa? GenerarPorDevolucion(Prestamo prestamo)
    {
        if (!prestamo.Devuelto)
            throw new ValidacionException("Solo se genera multa al devolver el préstamo.");

        var dias = prestamo.DiasAtraso(_reloj.Hoy);
        if (dias <= 0)
            return null;

        var multa = new Multa(prestamo.Id, prestamo.UsuarioId, dias, _calculadora.Calcular(dias), _reloj.Hoy);
        _multas.Agregar(multa);
        return multa;
    }

    public Multa Pagar(int multaId)
    {
        var multa = _multas.ObtenerPorId(multaId)
            ?? throw new ValidacionException("La multa seleccionada ya no existe.");
        multa.Pagar(_reloj.Hoy);
        _multas.Actualizar(multa);
        return multa;
    }

    public IReadOnlyList<Multa> PendientesDe(int usuarioId) =>
        _multas.ObtenerTodos().Where(m => m.UsuarioId == usuarioId && !m.Pagada).ToList();

    public decimal TotalPendienteDe(int usuarioId) => PendientesDe(usuarioId).Sum(m => m.Monto);

    public IReadOnlyList<MultaDetalle> Listar(bool? pagadas = null, string? texto = null)
    {
        var personas = _personas.ObtenerTodos().ToDictionary(p => p.Id);
        var prestamos = _prestamos.ObtenerTodos().ToDictionary(p => p.Id);
        var libros = _libros.ObtenerTodos().ToDictionary(l => l.Id);

        var detalle = _multas.ObtenerTodos()
            .Where(m => pagadas is null || m.Pagada == pagadas)
            .Select(m =>
            {
                var usuario = personas.TryGetValue(m.UsuarioId, out var p) ? p.Nombre : "(usuario eliminado)";
                var libro = prestamos.TryGetValue(m.PrestamoId, out var pr) && libros.TryGetValue(pr.LibroId, out var l)
                    ? l.Titulo : "(libro eliminado)";
                return new MultaDetalle(m.Id, usuario, libro, m.DiasAtraso, m.Monto,
                                        m.FechaGeneracion, m.Pagada, m.FechaPago);
            });

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var buscado = texto.Trim();
            detalle = detalle.Where(d =>
                d.Usuario.Contains(buscado, StringComparison.CurrentCultureIgnoreCase) ||
                d.Libro.Contains(buscado, StringComparison.CurrentCultureIgnoreCase));
        }

        return detalle.OrderBy(d => d.Pagada).ThenByDescending(d => d.FechaGeneracion).ToList();
    }
}
