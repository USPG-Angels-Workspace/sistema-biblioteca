using Biblioteca.Core.Interfaces;
using Biblioteca.Core.Modelos;
using Biblioteca.Core.Persistencia;
using Biblioteca.Core.Politicas;
using Biblioteca.Core.Servicios;

namespace Biblioteca.Core;

/// <summary>Punto de entrada de la lógica: arma los repositorios JSON y los servicios que la interfaz utiliza.</summary>
public class SistemaBiblioteca
{
    public LibroService Libros { get; }
    public UsuarioService Usuarios { get; }
    public PrestamoService Prestamos { get; }
    public DevolucionService Devoluciones { get; }
    public MultaService Multas { get; }
    public ReporteService Reportes { get; }
    public string CarpetaDatos { get; }

    public SistemaBiblioteca(string carpetaDatos, IReloj? reloj = null, ICalculadoraMulta? calculadora = null)
    {
        CarpetaDatos = carpetaDatos;
        reloj ??= new RelojSistema();
        calculadora ??= new MultaPorDia();

        var libros = new RepositorioJson<Libro>(Path.Combine(carpetaDatos, "libros.json"));
        var personas = new RepositorioJson<Persona>(Path.Combine(carpetaDatos, "usuarios.json"));
        var prestamos = new RepositorioJson<Prestamo>(Path.Combine(carpetaDatos, "prestamos.json"));
        var multas = new RepositorioJson<Multa>(Path.Combine(carpetaDatos, "multas.json"));

        Libros = new LibroService(libros, prestamos);
        Usuarios = new UsuarioService(personas, prestamos, multas, reloj);
        Multas = new MultaService(multas, prestamos, personas, libros, calculadora, reloj);
        Prestamos = new PrestamoService(libros, personas, prestamos, multas, reloj);
        Devoluciones = new DevolucionService(prestamos, libros, Multas, reloj);
        Reportes = new ReporteService(libros, personas, prestamos, multas, calculadora, reloj);
    }
}
