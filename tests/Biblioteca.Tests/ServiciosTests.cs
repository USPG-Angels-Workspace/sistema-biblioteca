using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Modelos;

namespace Biblioteca.Tests;

public class LibroServiceTests : IDisposable
{
    private readonly Escenario _e = new();
    public void Dispose() => _e.Dispose();

    [Fact]
    public void Registrar_asigna_ids_consecutivos()
    {
        var a = _e.NuevoLibro();
        var b = _e.NuevoLibro();
        Assert.Equal(1, a.Id);
        Assert.Equal(2, b.Id);
    }

    [Fact]
    public void Registrar_rechaza_isbn_duplicado_aunque_cambie_el_formato()
    {
        _e.Sistema.Libros.Registrar("978-9929-00-001-1", "A", "B", "C", 2000, CategoriaLibro.Otros, 1);
        var ex = Assert.Throws<ValidacionException>(() =>
            _e.Sistema.Libros.Registrar("9789929000011", "Otro", "B", "C", 2001, CategoriaLibro.Otros, 1));
        Assert.Contains("ISBN", ex.Message);
    }

    [Fact]
    public void Editar_permite_conservar_el_propio_isbn_y_rechaza_el_de_otro_libro()
    {
        var a = _e.NuevoLibro();
        var b = _e.NuevoLibro();
        _e.Sistema.Libros.Editar(a.Id, a.Isbn, "Título nuevo", "Autor", "Editorial", 2020, CategoriaLibro.Arte, 2);
        Assert.Equal("Título nuevo", _e.Sistema.Libros.ObtenerPorId(a.Id)!.Titulo);
        Assert.Throws<ValidacionException>(() =>
            _e.Sistema.Libros.Editar(b.Id, a.Isbn, "X", "Autor", "Editorial", 2020, CategoriaLibro.Arte, 1));
    }

    [Fact]
    public void Eliminar_funciona_sin_historial_y_se_bloquea_con_prestamos()
    {
        var libre = _e.NuevoLibro();
        var usado = _e.NuevoLibro();
        var lector = _e.NuevoLector();
        _e.Sistema.Prestamos.Prestar(usado.Id, lector.Id);

        _e.Sistema.Libros.Eliminar(libre.Id);
        Assert.Null(_e.Sistema.Libros.ObtenerPorId(libre.Id));
        Assert.Throws<ValidacionException>(() => _e.Sistema.Libros.Eliminar(usado.Id));
    }

    [Fact]
    public void Buscar_combina_texto_categoria_y_disponibilidad()
    {
        var a = _e.NuevoLibro(1, "Programación en C#");
        var b = _e.NuevoLibro(1, "Historia de Guatemala");
        _e.Sistema.Libros.Editar(b.Id, b.Isbn, "Historia de Guatemala", "Autor", "Editorial", 2020, CategoriaLibro.Historia, 1);
        var lector = _e.NuevoLector();
        _e.Sistema.Prestamos.Prestar(a.Id, lector.Id);

        Assert.Single(_e.Sistema.Libros.Buscar("c#"));
        Assert.Equal(2, _e.Sistema.Libros.Buscar(null).Count);
        Assert.Empty(_e.Sistema.Libros.Buscar("c#", soloDisponibles: true));
        Assert.Single(_e.Sistema.Libros.Buscar(null, CategoriaLibro.Tecnologia));
    }
}

public class UsuarioServiceTests : IDisposable
{
    private readonly Escenario _e = new();
    public void Dispose() => _e.Dispose();

    [Fact]
    public void Registrar_rechaza_documento_y_carnet_duplicados()
    {
        _e.Sistema.Usuarios.RegistrarLector("Ana", "2456789010101", "ana@ejemplo.com", "55123400", "C-1");
        Assert.Throws<ValidacionException>(() =>
            _e.Sistema.Usuarios.RegistrarLector("Otra", "2456789010101", "otra@ejemplo.com", "55123400", "C-2"));
        Assert.Throws<ValidacionException>(() =>
            _e.Sistema.Usuarios.RegistrarLector("Otra", "2456789020101", "otra@ejemplo.com", "55123400", "c-1"));
    }

    [Fact]
    public void Editar_actualiza_datos_y_permite_desactivar_sin_prestamos()
    {
        var lector = _e.NuevoLector();
        _e.Sistema.Usuarios.Editar(lector.Id, "Nombre nuevo", lector.Documento, lector.Correo, lector.Telefono, lector.Carnet, false);
        var guardado = _e.Sistema.Usuarios.ObtenerPorId(lector.Id)!;
        Assert.Equal("Nombre nuevo", guardado.Nombre);
        Assert.False(guardado.Activo);
    }

    [Fact]
    public void No_se_puede_desactivar_ni_eliminar_a_un_usuario_con_prestamos()
    {
        var lector = _e.NuevoLector();
        var libro = _e.NuevoLibro();
        _e.Sistema.Prestamos.Prestar(libro.Id, lector.Id);

        Assert.Throws<ValidacionException>(() =>
            _e.Sistema.Usuarios.Editar(lector.Id, lector.Nombre, lector.Documento, lector.Correo, lector.Telefono, lector.Carnet, false));
        Assert.Throws<ValidacionException>(() => _e.Sistema.Usuarios.Eliminar(lector.Id));
    }

    [Fact]
    public void Eliminar_funciona_para_un_usuario_sin_historial()
    {
        var lector = _e.NuevoLector();
        _e.Sistema.Usuarios.Eliminar(lector.Id);
        Assert.Empty(_e.Sistema.Usuarios.Listar());
    }

    [Fact]
    public void Buscar_filtra_por_texto_y_por_tipo()
    {
        _e.NuevoLector("María López");
        _e.NuevoBibliotecario();
        Assert.Single(_e.Sistema.Usuarios.Buscar("maría"));
        Assert.Single(_e.Sistema.Usuarios.Buscar(null, "Bibliotecario"));
        Assert.Equal(2, _e.Sistema.Usuarios.Buscar(null).Count);
    }
}

public class PrestamoFlujoTests : IDisposable
{
    private readonly Escenario _e = new();
    public void Dispose() => _e.Dispose();

    [Fact]
    public void Prestar_descuenta_un_ejemplar_y_fija_el_vencimiento_segun_el_tipo_de_usuario()
    {
        var libro = _e.NuevoLibro(2);
        var lector = _e.NuevoLector();
        var biblio = _e.NuevoBibliotecario();

        var p1 = _e.Sistema.Prestamos.Prestar(libro.Id, lector.Id);
        var p2 = _e.Sistema.Prestamos.Prestar(libro.Id, biblio.Id);

        Assert.Equal(_e.Reloj.Hoy.AddDays(7), p1.FechaVencimiento);
        Assert.Equal(_e.Reloj.Hoy.AddDays(14), p2.FechaVencimiento);
        Assert.Equal(0, _e.Sistema.Libros.ObtenerPorId(libro.Id)!.CopiasDisponibles);
    }

    [Fact]
    public void Prestar_falla_sin_ejemplares_disponibles()
    {
        var libro = _e.NuevoLibro(1);
        _e.Sistema.Prestamos.Prestar(libro.Id, _e.NuevoLector().Id);
        var ex = Assert.Throws<ValidacionException>(() => _e.Sistema.Prestamos.Prestar(libro.Id, _e.NuevoLector().Id));
        Assert.Contains("disponibles", ex.Message);
    }

    [Fact]
    public void Prestar_respeta_el_limite_del_tipo_de_usuario()
    {
        var lector = _e.NuevoLector();
        for (var i = 0; i < 3; i++)
            _e.Sistema.Prestamos.Prestar(_e.NuevoLibro().Id, lector.Id);

        var ex = Assert.Throws<ValidacionException>(() => _e.Sistema.Prestamos.Prestar(_e.NuevoLibro().Id, lector.Id));
        Assert.Contains("límite", ex.Message);
    }

    [Fact]
    public void Prestar_rechaza_usuarios_inactivos_y_el_mismo_libro_dos_veces()
    {
        var libro = _e.NuevoLibro(3);
        var lector = _e.NuevoLector();
        _e.Sistema.Prestamos.Prestar(libro.Id, lector.Id);
        Assert.Throws<ValidacionException>(() => _e.Sistema.Prestamos.Prestar(libro.Id, lector.Id));

        var inactivo = _e.NuevoLector();
        _e.Sistema.Usuarios.Editar(inactivo.Id, inactivo.Nombre, inactivo.Documento, inactivo.Correo,
                                   inactivo.Telefono, inactivo.Carnet, false);
        Assert.Throws<ValidacionException>(() => _e.Sistema.Prestamos.Prestar(libro.Id, inactivo.Id));
    }

    [Fact]
    public void Prestar_rechaza_a_quien_tiene_un_prestamo_atrasado()
    {
        var lector = _e.NuevoLector();
        _e.Sistema.Prestamos.Prestar(_e.NuevoLibro().Id, lector.Id);
        _e.Reloj.Avanzar(8);
        var ex = Assert.Throws<ValidacionException>(() => _e.Sistema.Prestamos.Prestar(_e.NuevoLibro().Id, lector.Id));
        Assert.Contains("atrasados", ex.Message);
    }

    [Fact]
    public void Renovar_extiende_el_plazo_y_respeta_el_maximo()
    {
        var lector = _e.NuevoLector();
        var prestamo = _e.Sistema.Prestamos.Prestar(_e.NuevoLibro().Id, lector.Id);
        var original = prestamo.FechaVencimiento;

        _e.Sistema.Prestamos.Renovar(prestamo.Id);
        _e.Sistema.Prestamos.Renovar(prestamo.Id);
        Assert.Equal(original.AddDays(14), _e.Sistema.Prestamos.ObtenerPorId(prestamo.Id)!.FechaVencimiento);
        Assert.Throws<ValidacionException>(() => _e.Sistema.Prestamos.Renovar(prestamo.Id));
    }

    [Fact]
    public void Devolver_a_tiempo_libera_el_ejemplar_y_no_genera_multa()
    {
        var libro = _e.NuevoLibro(1);
        var prestamo = _e.Sistema.Prestamos.Prestar(libro.Id, _e.NuevoLector().Id);
        _e.Reloj.Avanzar(7);

        var resultado = _e.Sistema.Devoluciones.Registrar(prestamo.Id);

        Assert.Null(resultado.Multa);
        Assert.True(resultado.Prestamo.Devuelto);
        Assert.True(_e.Sistema.Libros.ObtenerPorId(libro.Id)!.Disponible);
        Assert.Empty(_e.Sistema.Multas.Listar());
    }

    [Fact]
    public void Devolver_con_atraso_genera_multa_y_bloquea_nuevos_prestamos_hasta_pagarla()
    {
        var lector = _e.NuevoLector();
        var prestamo = _e.Sistema.Prestamos.Prestar(_e.NuevoLibro().Id, lector.Id);
        _e.Reloj.Avanzar(7 + 4);

        var resultado = _e.Sistema.Devoluciones.Registrar(prestamo.Id);

        Assert.NotNull(resultado.Multa);
        Assert.Equal(4, resultado.Multa!.DiasAtraso);
        Assert.Equal(8m, resultado.Multa.Monto);
        Assert.Equal(8m, _e.Sistema.Multas.TotalPendienteDe(lector.Id));
        Assert.Throws<ValidacionException>(() => _e.Sistema.Prestamos.Prestar(_e.NuevoLibro().Id, lector.Id));

        _e.Sistema.Multas.Pagar(resultado.Multa.Id);

        Assert.Equal(0m, _e.Sistema.Multas.TotalPendienteDe(lector.Id));
        var nuevo = _e.Sistema.Prestamos.Prestar(_e.NuevoLibro().Id, lector.Id);
        Assert.NotNull(nuevo);
    }

    [Fact]
    public void Devolver_dos_veces_o_pagar_dos_veces_es_un_error()
    {
        var prestamo = _e.Sistema.Prestamos.Prestar(_e.NuevoLibro().Id, _e.NuevoLector().Id);
        _e.Reloj.Avanzar(10);
        var resultado = _e.Sistema.Devoluciones.Registrar(prestamo.Id);
        Assert.Throws<ValidacionException>(() => _e.Sistema.Devoluciones.Registrar(prestamo.Id));

        _e.Sistema.Multas.Pagar(resultado.Multa!.Id);
        Assert.Throws<ValidacionException>(() => _e.Sistema.Multas.Pagar(resultado.Multa.Id));
    }

    [Fact]
    public void Listar_resuelve_nombres_estados_y_filtra_pendientes()
    {
        var libro = _e.NuevoLibro(2, "Rayuela");
        var lector = _e.NuevoLector("Ana Pérez");
        var otro = _e.NuevoLector("Luis Gómez");
        var devuelto = _e.Sistema.Prestamos.Prestar(libro.Id, lector.Id);
        _e.Sistema.Devoluciones.Registrar(devuelto.Id);
        _e.Sistema.Prestamos.Prestar(libro.Id, otro.Id);
        _e.Reloj.Avanzar(9);

        var todos = _e.Sistema.Prestamos.Listar();
        var pendientes = _e.Sistema.Prestamos.Listar(soloPendientes: true);

        Assert.Equal(2, todos.Count);
        var unico = Assert.Single(pendientes);
        Assert.Equal("Rayuela", unico.Libro);
        Assert.Equal("Luis Gómez", unico.Usuario);
        Assert.Equal(EstadoPrestamo.Atrasado, unico.Estado);
        Assert.Equal(2, unico.DiasAtraso);
        Assert.Single(_e.Sistema.Prestamos.Listar(texto: "ana"));
    }
}

public class ReporteServiceTests : IDisposable
{
    private readonly Escenario _e = new();
    public void Dispose() => _e.Dispose();

    [Fact]
    public void Los_reportes_reflejan_disponibilidad_atrasos_y_multas()
    {
        var agotado = _e.NuevoLibro(1, "Agotado");
        _e.NuevoLibro(2, "Disponible");
        var lector = _e.NuevoLector("Ana");
        var moroso = _e.NuevoLector("Luis");
        _e.Sistema.Prestamos.Prestar(agotado.Id, lector.Id);
        var atrasado = _e.Sistema.Prestamos.Prestar(_e.NuevoLibro(1, "Atrasado").Id, moroso.Id);
        _e.Reloj.Avanzar(10);

        var disponibles = _e.Sistema.Reportes.LibrosDisponibles();
        var activos = _e.Sistema.Reportes.PrestamosActivos();
        var atrasados = _e.Sistema.Reportes.LibrosAtrasados();

        Assert.Single(disponibles.Filas);
        Assert.Equal("Disponible", disponibles.Filas[0][1]);
        Assert.Equal(2, activos.Filas.Count);
        Assert.Equal(2, atrasados.Filas.Count);
        Assert.Equal("Q6.00", atrasados.Filas[0][4]);

        _e.Sistema.Devoluciones.Registrar(atrasado.Id);
        var multas = _e.Sistema.Reportes.Multas();
        Assert.Single(multas.Filas);
        Assert.Contains("Pendiente de cobro: Q6.00", multas.Resumen);
        Assert.Equal(2, _e.Sistema.Reportes.Usuarios().Filas.Count);
    }

    [Fact]
    public void ACsv_escapa_comas_y_comillas()
    {
        _e.NuevoLibro(1, "Título, con \"comillas\"");
        var csv = _e.Sistema.Reportes.LibrosDisponibles().ACsv();
        Assert.Contains("\"Título, con \"\"comillas\"\"\"", csv);
        Assert.StartsWith("ISBN,Título,Autor,Categoría,Ejemplares disponibles", csv);
    }
}
