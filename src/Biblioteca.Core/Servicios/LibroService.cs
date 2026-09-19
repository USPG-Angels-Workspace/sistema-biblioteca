using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Interfaces;
using Biblioteca.Core.Modelos;

namespace Biblioteca.Core.Servicios;

public class LibroService
{
    private readonly IRepositorio<Libro> _libros;
    private readonly IRepositorio<Prestamo> _prestamos;

    public LibroService(IRepositorio<Libro> libros, IRepositorio<Prestamo> prestamos)
    {
        _libros = libros;
        _prestamos = prestamos;
    }

    public Libro Registrar(string isbn, string titulo, string autor, string editorial, int anio,
                           CategoriaLibro categoria, int copias)
    {
        var libro = new Libro(isbn, titulo, autor, editorial, anio, categoria, copias);
        ValidarIsbnUnico(libro.Isbn, null);
        _libros.Agregar(libro);
        return libro;
    }

    public Libro Editar(int id, string isbn, string titulo, string autor, string editorial, int anio,
                        CategoriaLibro categoria, int copiasTotales)
    {
        var libro = ObtenerPorId(id) ?? throw new ValidacionException("El libro seleccionado ya no existe.");
        ValidarIsbnUnico(Validador.Isbn(isbn), id);
        libro.Actualizar(isbn, titulo, autor, editorial, anio, categoria, copiasTotales);
        _libros.Actualizar(libro);
        return libro;
    }

    public void Eliminar(int id)
    {
        var libro = ObtenerPorId(id) ?? throw new ValidacionException("El libro seleccionado ya no existe.");
        if (_prestamos.ObtenerTodos().Any(p => p.LibroId == id))
            throw new ValidacionException(
                $"No se puede eliminar «{libro.Titulo}» porque tiene préstamos registrados en el historial.");
        _libros.Eliminar(id);
    }

    public Libro? ObtenerPorId(int id) => _libros.ObtenerPorId(id);

    public IReadOnlyList<Libro> Listar() =>
        _libros.ObtenerTodos().OrderBy(l => l.Titulo, StringComparer.CurrentCultureIgnoreCase).ToList();

    public IReadOnlyList<Libro> Buscar(string? texto, CategoriaLibro? categoria = null, bool soloDisponibles = false) =>
        Listar()
            .Where(l => l.Coincide(texto))
            .Where(l => categoria is null || l.Categoria == categoria)
            .Where(l => !soloDisponibles || l.Disponible)
            .ToList();

    private void ValidarIsbnUnico(string isbnNormalizado, int? idActual)
    {
        if (_libros.ObtenerTodos().Any(l => l.Isbn == isbnNormalizado && l.Id != idActual))
            throw new ValidacionException("Ya existe un libro registrado con ese ISBN.");
    }
}
