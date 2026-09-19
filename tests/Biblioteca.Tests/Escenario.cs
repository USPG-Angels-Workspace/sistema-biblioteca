using Biblioteca.Core;
using Biblioteca.Core.Interfaces;
using Biblioteca.Core.Modelos;

namespace Biblioteca.Tests;

public class RelojFalso : IReloj
{
    public DateTime Hoy { get; set; } = new(2026, 9, 1);

    public void Avanzar(int dias) => Hoy = Hoy.AddDays(dias);
}

/// <summary>Sistema completo sobre una carpeta temporal, con reloj controlable.</summary>
public sealed class Escenario : IDisposable
{
    private static int _contador;

    public string Carpeta { get; }
    public RelojFalso Reloj { get; } = new();
    public SistemaBiblioteca Sistema { get; private set; }

    public Escenario()
    {
        Carpeta = Path.Combine(Path.GetTempPath(), "biblioteca-tests-" + Guid.NewGuid().ToString("N"));
        Sistema = new SistemaBiblioteca(Carpeta, Reloj);
    }

    public SistemaBiblioteca Reabrir()
    {
        Sistema = new SistemaBiblioteca(Carpeta, Reloj);
        return Sistema;
    }

    public Libro NuevoLibro(int copias = 1, string? titulo = null)
    {
        var n = Interlocked.Increment(ref _contador);
        return Sistema.Libros.Registrar($"978992900{n:D4}", titulo ?? $"Libro {n}", "Autor", "Editorial",
                                        2020, CategoriaLibro.Tecnologia, copias);
    }

    public Lector NuevoLector(string? nombre = null)
    {
        var n = Interlocked.Increment(ref _contador);
        return Sistema.Usuarios.RegistrarLector(nombre ?? $"Lector {n}", $"24567{n:D8}", $"lector{n}@ejemplo.com",
                                                "55123400", $"C-{n}");
    }

    public Bibliotecario NuevoBibliotecario()
    {
        var n = Interlocked.Increment(ref _contador);
        return Sistema.Usuarios.RegistrarBibliotecario($"Bibliotecario {n}", $"24567{n:D8}",
                                                       $"biblio{n}@ejemplo.com", "55123400", "Encargado");
    }

    public void Dispose()
    {
        if (Directory.Exists(Carpeta))
            Directory.Delete(Carpeta, recursive: true);
    }
}
