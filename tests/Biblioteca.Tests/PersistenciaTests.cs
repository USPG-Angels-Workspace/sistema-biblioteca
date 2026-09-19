using Biblioteca.Core;
using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Modelos;

namespace Biblioteca.Tests;

public class PersistenciaTests : IDisposable
{
    private readonly Escenario _e = new();
    public void Dispose() => _e.Dispose();

    [Fact]
    public void Los_datos_sobreviven_al_cerrar_y_volver_a_abrir_el_sistema()
    {
        var libro = _e.NuevoLibro(2, "Persistente");
        var lector = _e.NuevoLector("Ana");
        var biblio = _e.NuevoBibliotecario();
        _e.Sistema.Prestamos.Prestar(libro.Id, lector.Id);
        _e.Reloj.Avanzar(9);
        var prestamoBiblio = _e.Sistema.Prestamos.Prestar(libro.Id, biblio.Id);
        _e.Reloj.Avanzar(1);
        _e.Sistema.Devoluciones.Registrar(prestamoBiblio.Id);

        var reabierto = _e.Reabrir();

        Assert.Equal("Persistente", reabierto.Libros.ObtenerPorId(libro.Id)!.Titulo);
        Assert.Equal(1, reabierto.Libros.ObtenerPorId(libro.Id)!.CopiasDisponibles);
        Assert.Equal(2, reabierto.Prestamos.Listar().Count);
        Assert.Single(reabierto.Prestamos.Listar(soloPendientes: true));
    }

    [Fact]
    public void El_polimorfismo_de_usuarios_se_conserva_en_el_archivo()
    {
        _e.NuevoLector("Ana");
        _e.NuevoBibliotecario();

        var usuarios = _e.Reabrir().Usuarios.Listar();

        Assert.Single(usuarios.OfType<Lector>());
        Assert.Single(usuarios.OfType<Bibliotecario>());
        Assert.Equal(3, usuarios.OfType<Lector>().Single().LimitePrestamos);
        var json = File.ReadAllText(Path.Combine(_e.Carpeta, "usuarios.json"));
        Assert.Contains("\"tipo\": \"lector\"", json);
        Assert.DoesNotContain("limitePrestamos", json);
    }

    [Fact]
    public void Las_multas_y_su_estado_de_pago_se_recuperan()
    {
        var prestamo = _e.Sistema.Prestamos.Prestar(_e.NuevoLibro().Id, _e.NuevoLector().Id);
        _e.Reloj.Avanzar(12);
        var multa = _e.Sistema.Devoluciones.Registrar(prestamo.Id).Multa!;
        _e.Sistema.Multas.Pagar(multa.Id);

        var recuperada = Assert.Single(_e.Reabrir().Multas.Listar());
        Assert.True(recuperada.Pagada);
        Assert.Equal(multa.Monto, recuperada.Monto);
        Assert.Equal(_e.Reloj.Hoy, recuperada.FechaPago);
    }

    [Fact]
    public void Los_ids_nuevos_toman_el_maximo_existente_mas_uno_despues_de_reabrir()
    {
        _e.NuevoLibro();
        var segundo = _e.NuevoLibro();
        _e.NuevoLibro();
        _e.Sistema.Libros.Eliminar(segundo.Id);

        var nuevo = _e.Reabrir().Libros.Registrar("9789929000099", "Nuevo", "A", "E", 2020, CategoriaLibro.Otros, 1);

        Assert.Equal(4, nuevo.Id);
    }

    [Fact]
    public void Guardar_no_deja_archivos_temporales()
    {
        _e.NuevoLibro();
        Assert.Empty(Directory.GetFiles(_e.Carpeta, "*.tmp"));
        Assert.True(File.Exists(Path.Combine(_e.Carpeta, "libros.json")));
    }

    [Fact]
    public void Sin_archivos_el_sistema_arranca_vacio()
    {
        Assert.Empty(_e.Sistema.Libros.Listar());
        Assert.Empty(_e.Sistema.Usuarios.Listar());
        Assert.Empty(_e.Sistema.Prestamos.Listar());
    }

    [Fact]
    public void Un_archivo_danado_produce_un_error_de_almacenamiento_claro()
    {
        Directory.CreateDirectory(_e.Carpeta);
        File.WriteAllText(Path.Combine(_e.Carpeta, "libros.json"), "{ esto no es json ");

        var ex = Assert.Throws<AlmacenamientoException>(() => new SistemaBiblioteca(_e.Carpeta, _e.Reloj));
        Assert.Contains("libros.json", ex.Message);
    }

    [Fact]
    public void Un_archivo_vacio_se_trata_como_coleccion_vacia()
    {
        Directory.CreateDirectory(_e.Carpeta);
        File.WriteAllText(Path.Combine(_e.Carpeta, "libros.json"), "   ");
        Assert.Empty(new SistemaBiblioteca(_e.Carpeta, _e.Reloj).Libros.Listar());
    }
}

public class DatosDePruebaTests : IDisposable
{
    private readonly string _carpeta;
    private readonly SistemaBiblioteca _sistema;

    public DatosDePruebaTests()
    {
        _carpeta = Path.Combine(Path.GetTempPath(), "biblioteca-datos-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_carpeta);
        var origen = Path.Combine(AppContext.BaseDirectory, "datos");
        foreach (var archivo in Directory.GetFiles(origen, "*.json"))
            File.Copy(archivo, Path.Combine(_carpeta, Path.GetFileName(archivo)));
        _sistema = new SistemaBiblioteca(_carpeta, new RelojFalso { Hoy = new DateTime(2026, 9, 19) });
    }

    public void Dispose() => Directory.Delete(_carpeta, recursive: true);

    [Fact]
    public void Los_datos_de_prueba_cargan_con_todas_las_entidades()
    {
        Assert.True(_sistema.Libros.Listar().Count >= 10);
        Assert.Contains(_sistema.Usuarios.Listar(), u => u is Lector);
        Assert.Contains(_sistema.Usuarios.Listar(), u => u is Bibliotecario);
        Assert.NotEmpty(_sistema.Prestamos.Listar());
        Assert.NotEmpty(_sistema.Multas.Listar());
    }

    [Fact]
    public void Los_ejemplares_disponibles_coinciden_con_los_prestamos_pendientes()
    {
        var pendientes = _sistema.Prestamos.Listar(soloPendientes: true);
        foreach (var libro in _sistema.Libros.Listar())
        {
            var prestados = pendientes.Count(p => p.Libro == libro.Titulo);
            Assert.Equal(libro.CopiasTotales - prestados, libro.CopiasDisponibles);
        }
    }

    [Fact]
    public void Los_datos_de_prueba_incluyen_casos_para_demostrar_todos_los_estados()
    {
        var prestamos = _sistema.Prestamos.Listar();
        Assert.Contains(prestamos, p => p.Estado == EstadoPrestamo.Activo);
        Assert.Contains(prestamos, p => p.Estado == EstadoPrestamo.Atrasado);
        Assert.Contains(prestamos, p => p.Estado == EstadoPrestamo.Devuelto);
        Assert.Contains(_sistema.Multas.Listar(), m => m.Pagada);
        Assert.Contains(_sistema.Multas.Listar(), m => !m.Pagada);
        Assert.Contains(_sistema.Libros.Listar(), l => !l.Disponible);
    }

    [Fact]
    public void Ningun_prestamo_ni_multa_apunta_a_registros_inexistentes()
    {
        Assert.DoesNotContain(_sistema.Prestamos.Listar(), p => p.Libro.StartsWith('(') || p.Usuario.StartsWith('('));
        Assert.DoesNotContain(_sistema.Multas.Listar(), m => m.Libro.StartsWith('(') || m.Usuario.StartsWith('('));
    }

    [Fact]
    public void Todos_los_reportes_se_generan_con_los_datos_de_prueba()
    {
        Assert.NotEmpty(_sistema.Reportes.LibrosDisponibles().Filas);
        Assert.NotEmpty(_sistema.Reportes.PrestamosActivos().Filas);
        Assert.NotEmpty(_sistema.Reportes.LibrosAtrasados().Filas);
        Assert.NotEmpty(_sistema.Reportes.Usuarios().Filas);
        Assert.NotEmpty(_sistema.Reportes.Multas().Filas);
    }
}
