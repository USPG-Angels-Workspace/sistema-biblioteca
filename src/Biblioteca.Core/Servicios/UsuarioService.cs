using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Interfaces;
using Biblioteca.Core.Modelos;

namespace Biblioteca.Core.Servicios;

public class UsuarioService
{
    private readonly IRepositorio<Persona> _personas;
    private readonly IRepositorio<Prestamo> _prestamos;
    private readonly IRepositorio<Multa> _multas;
    private readonly IReloj _reloj;

    public UsuarioService(IRepositorio<Persona> personas, IRepositorio<Prestamo> prestamos,
                          IRepositorio<Multa> multas, IReloj reloj)
    {
        _personas = personas;
        _prestamos = prestamos;
        _multas = multas;
        _reloj = reloj;
    }

    public Lector RegistrarLector(string nombre, string documento, string correo, string telefono, string carnet)
    {
        var lector = new Lector(nombre, documento, correo, telefono, carnet, _reloj.Hoy);
        ValidarUnicos(lector.Documento, lector, null);
        _personas.Agregar(lector);
        return lector;
    }

    public Bibliotecario RegistrarBibliotecario(string nombre, string documento, string correo,
                                                string telefono, string cargo)
    {
        var bibliotecario = new Bibliotecario(nombre, documento, correo, telefono, cargo, _reloj.Hoy);
        ValidarUnicos(bibliotecario.Documento, bibliotecario, null);
        _personas.Agregar(bibliotecario);
        return bibliotecario;
    }

    public Persona Editar(int id, string nombre, string documento, string correo, string telefono,
                          string detalle, bool activo)
    {
        var persona = ObtenerPorId(id) ?? throw new ValidacionException("El usuario seleccionado ya no existe.");
        var documentoNormalizado = Validador.Documento(documento);
        if (persona is Lector)
            ValidarCarnetUnico(Validador.TextoRequerido(detalle, "Carnet", 20), id);
        ValidarDocumentoUnico(documentoNormalizado, id);

        if (!activo && persona.Activo && ContarPrestamosActivos(id) > 0)
            throw new ValidacionException("No se puede desactivar un usuario que tiene préstamos activos.");

        persona.Actualizar(nombre, documento, correo, telefono, detalle, activo);
        _personas.Actualizar(persona);
        return persona;
    }

    public void Eliminar(int id)
    {
        var persona = ObtenerPorId(id) ?? throw new ValidacionException("El usuario seleccionado ya no existe.");
        if (_prestamos.ObtenerTodos().Any(p => p.UsuarioId == id) || _multas.ObtenerTodos().Any(m => m.UsuarioId == id))
            throw new ValidacionException(
                $"No se puede eliminar a «{persona.Nombre}» porque tiene préstamos o multas en el historial. " +
                "Puede desactivarlo desde la edición.");
        _personas.Eliminar(id);
    }

    public Persona? ObtenerPorId(int id) => _personas.ObtenerPorId(id);

    public IReadOnlyList<Persona> Listar() =>
        _personas.ObtenerTodos().OrderBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase).ToList();

    public IReadOnlyList<Persona> Buscar(string? texto, string? tipoUsuario = null) =>
        Listar()
            .Where(p => p.Coincide(texto))
            .Where(p => string.IsNullOrEmpty(tipoUsuario) || p.TipoUsuario == tipoUsuario)
            .ToList();

    private int ContarPrestamosActivos(int usuarioId) =>
        _prestamos.ObtenerTodos().Count(p => p.UsuarioId == usuarioId && !p.Devuelto);

    private void ValidarUnicos(string documentoNormalizado, Persona nueva, int? idActual)
    {
        ValidarDocumentoUnico(documentoNormalizado, idActual);
        if (nueva is Lector lector)
            ValidarCarnetUnico(lector.Carnet, idActual);
    }

    private void ValidarDocumentoUnico(string documentoNormalizado, int? idActual)
    {
        if (_personas.ObtenerTodos().Any(p => p.Documento == documentoNormalizado && p.Id != idActual))
            throw new ValidacionException("Ya existe un usuario registrado con ese documento (DPI).");
    }

    private void ValidarCarnetUnico(string carnet, int? idActual)
    {
        if (_personas.ObtenerTodos().OfType<Lector>()
            .Any(l => string.Equals(l.Carnet, carnet, StringComparison.OrdinalIgnoreCase) && l.Id != idActual))
            throw new ValidacionException("Ya existe un lector registrado con ese carnet.");
    }
}
