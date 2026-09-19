using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Interfaces;
using Biblioteca.Core.Modelos;

namespace Biblioteca.Core.Servicios;

public class DevolucionService
{
    private readonly IRepositorio<Prestamo> _prestamos;
    private readonly IRepositorio<Libro> _libros;
    private readonly MultaService _multas;
    private readonly IReloj _reloj;

    public DevolucionService(IRepositorio<Prestamo> prestamos, IRepositorio<Libro> libros,
                             MultaService multas, IReloj reloj)
    {
        _prestamos = prestamos;
        _libros = libros;
        _multas = multas;
        _reloj = reloj;
    }

    public ResultadoDevolucion Registrar(int prestamoId)
    {
        var prestamo = _prestamos.ObtenerPorId(prestamoId)
            ?? throw new ValidacionException("El préstamo seleccionado ya no existe.");
        var libro = _libros.ObtenerPorId(prestamo.LibroId)
            ?? throw new ValidacionException("El libro del préstamo ya no existe.");

        prestamo.RegistrarDevolucion(_reloj.Hoy);
        libro.DevolverCopia();
        _prestamos.Actualizar(prestamo);
        _libros.Actualizar(libro);

        var multa = _multas.GenerarPorDevolucion(prestamo);
        return new ResultadoDevolucion(prestamo, multa);
    }
}
