using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Modelos;
using Biblioteca.Core.Politicas;

namespace Biblioteca.Tests;

public class LibroTests
{
    private static Libro Crear(int copias = 2) =>
        new("978-9929-00-001-1", "Clean Code", "Robert C. Martin", "Prentice Hall", 2008, CategoriaLibro.Tecnologia, copias);

    [Fact]
    public void Constructor_normaliza_isbn_y_deja_todos_los_ejemplares_disponibles()
    {
        var libro = Crear(3);
        Assert.Equal("9789929000011", libro.Isbn);
        Assert.Equal(3, libro.CopiasDisponibles);
        Assert.True(libro.Disponible);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("97899290000AB")]
    [InlineData("")]
    public void Constructor_rechaza_isbn_invalido(string isbn)
    {
        Assert.Throws<ValidacionException>(() =>
            new Libro(isbn, "T", "A", "E", 2000, CategoriaLibro.Otros, 1));
    }

    [Theory]
    [InlineData(999)]
    [InlineData(3000)]
    public void Constructor_rechaza_anio_fuera_de_rango(int anio)
    {
        Assert.Throws<ValidacionException>(() =>
            new Libro("9789929000011", "T", "A", "E", anio, CategoriaLibro.Otros, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    public void Constructor_rechaza_cantidad_de_ejemplares_invalida(int copias)
    {
        Assert.Throws<ValidacionException>(() =>
            new Libro("9789929000011", "T", "A", "E", 2000, CategoriaLibro.Otros, copias));
    }

    [Fact]
    public void Prestar_y_devolver_ajustan_la_disponibilidad()
    {
        var libro = Crear(1);
        libro.PrestarCopia();
        Assert.False(libro.Disponible);
        Assert.Throws<ValidacionException>(libro.PrestarCopia);
        libro.DevolverCopia();
        Assert.True(libro.Disponible);
        Assert.Throws<ValidacionException>(libro.DevolverCopia);
    }

    [Fact]
    public void Actualizar_no_permite_bajar_de_los_ejemplares_prestados()
    {
        var libro = Crear(3);
        libro.PrestarCopia();
        libro.PrestarCopia();
        Assert.Throws<ValidacionException>(() =>
            libro.Actualizar("9789929000011", "Clean Code", "A", "E", 2008, CategoriaLibro.Tecnologia, 1));
    }

    [Fact]
    public void Actualizar_conserva_los_ejemplares_prestados_al_cambiar_el_total()
    {
        var libro = Crear(3);
        libro.PrestarCopia();
        libro.Actualizar("9789929000011", "Clean Code", "A", "E", 2008, CategoriaLibro.Tecnologia, 5);
        Assert.Equal(5, libro.CopiasTotales);
        Assert.Equal(4, libro.CopiasDisponibles);
    }

    [Fact]
    public void Actualizar_con_datos_invalidos_no_modifica_el_libro()
    {
        var libro = Crear(2);
        Assert.Throws<ValidacionException>(() =>
            libro.Actualizar("9789929000011", "Nuevo título", "", "E", 2008, CategoriaLibro.Arte, 2));
        Assert.Equal("Clean Code", libro.Titulo);
        Assert.Equal(CategoriaLibro.Tecnologia, libro.Categoria);
    }

    [Fact]
    public void Coincide_busca_por_titulo_autor_e_isbn_sin_distinguir_mayusculas()
    {
        var libro = Crear();
        Assert.True(libro.Coincide("clean"));
        Assert.True(libro.Coincide("MARTIN"));
        Assert.True(libro.Coincide("978-9929-00-001-1"));
        Assert.False(libro.Coincide("quijote"));
        Assert.True(libro.Coincide("  "));
    }
}

public class PersonaTests
{
    [Fact]
    public void Lector_y_bibliotecario_tienen_reglas_de_prestamo_distintas()
    {
        Persona lector = new Lector("Ana", "2456789010101", "ana@ejemplo.com", "55123400", "C-1", DateTime.Today);
        Persona biblio = new Bibliotecario("Rosa", "2456789020101", "rosa@ejemplo.com", "55123400", "Jefa", DateTime.Today);

        Assert.Equal(3, lector.LimitePrestamos);
        Assert.Equal(7, lector.DiasPrestamo);
        Assert.Equal(5, biblio.LimitePrestamos);
        Assert.Equal(14, biblio.DiasPrestamo);
        Assert.Equal("Carnet", lector.EtiquetaDetalle);
        Assert.Equal("Cargo", biblio.EtiquetaDetalle);
    }

    [Theory]
    [InlineData("", "2456789010101", "a@b.com", "55123400")]
    [InlineData("Ana", "123", "a@b.com", "55123400")]
    [InlineData("Ana", "245678901010A", "a@b.com", "55123400")]
    [InlineData("Ana", "2456789010101", "correo-sin-arroba", "55123400")]
    [InlineData("Ana", "2456789010101", "a@b.com", "12")]
    public void Constructor_valida_los_datos_personales(string nombre, string dpi, string correo, string telefono)
    {
        Assert.Throws<ValidacionException>(() =>
            new Lector(nombre, dpi, correo, telefono, "C-1", DateTime.Today));
    }

    [Fact]
    public void Lector_exige_carnet()
    {
        Assert.Throws<ValidacionException>(() =>
            new Lector("Ana", "2456789010101", "ana@ejemplo.com", "55123400", " ", DateTime.Today));
    }

    [Fact]
    public void Actualizar_reemplaza_los_datos_y_el_detalle_segun_el_tipo()
    {
        var lector = new Lector("Ana", "2456789010101", "ana@ejemplo.com", "55123400", "C-1", DateTime.Today);
        lector.Actualizar("Ana Lucía", "2456789010101", "nueva@ejemplo.com", "5512-3499", "C-99", false);
        Assert.Equal("Ana Lucía", lector.Nombre);
        Assert.Equal("55123499", lector.Telefono);
        Assert.Equal("C-99", lector.Carnet);
        Assert.False(lector.Activo);
    }
}

public class PrestamoTests
{
    private static readonly DateTime Inicio = new(2026, 9, 1);

    [Fact]
    public void Vencimiento_es_la_fecha_del_prestamo_mas_el_plazo()
    {
        var prestamo = new Prestamo(1, 1, Inicio, 7);
        Assert.Equal(new DateTime(2026, 9, 8), prestamo.FechaVencimiento);
    }

    [Fact]
    public void Estado_cambia_a_atrasado_solo_despues_del_vencimiento()
    {
        var prestamo = new Prestamo(1, 1, Inicio, 7);
        Assert.Equal(EstadoPrestamo.Activo, prestamo.ObtenerEstado(new DateTime(2026, 9, 8)));
        Assert.Equal(EstadoPrestamo.Atrasado, prestamo.ObtenerEstado(new DateTime(2026, 9, 9)));
        prestamo.RegistrarDevolucion(new DateTime(2026, 9, 9));
        Assert.Equal(EstadoPrestamo.Devuelto, prestamo.ObtenerEstado(new DateTime(2026, 12, 1)));
    }

    [Fact]
    public void DiasAtraso_se_congela_en_la_fecha_de_devolucion()
    {
        var prestamo = new Prestamo(1, 1, Inicio, 7);
        prestamo.RegistrarDevolucion(new DateTime(2026, 9, 12));
        Assert.Equal(4, prestamo.DiasAtraso(new DateTime(2026, 10, 30)));
    }

    [Fact]
    public void Renovar_extiende_el_vencimiento_hasta_el_maximo_permitido()
    {
        var prestamo = new Prestamo(1, 1, Inicio, 7);
        prestamo.Renovar(7, Inicio.AddDays(2));
        prestamo.Renovar(7, Inicio.AddDays(3));
        Assert.Equal(new DateTime(2026, 9, 22), prestamo.FechaVencimiento);
        Assert.Equal(2, prestamo.Renovaciones);
        Assert.Throws<ValidacionException>(() => prestamo.Renovar(7, Inicio.AddDays(4)));
    }

    [Fact]
    public void No_se_puede_renovar_un_prestamo_vencido_ni_devuelto()
    {
        var vencido = new Prestamo(1, 1, Inicio, 7);
        Assert.Throws<ValidacionException>(() => vencido.Renovar(7, Inicio.AddDays(8)));

        var devuelto = new Prestamo(1, 1, Inicio, 7);
        devuelto.RegistrarDevolucion(Inicio.AddDays(1));
        Assert.Throws<ValidacionException>(() => devuelto.Renovar(7, Inicio.AddDays(2)));
    }

    [Fact]
    public void No_se_puede_devolver_dos_veces_ni_antes_del_prestamo()
    {
        var prestamo = new Prestamo(1, 1, Inicio, 7);
        Assert.Throws<ValidacionException>(() => prestamo.RegistrarDevolucion(Inicio.AddDays(-1)));
        prestamo.RegistrarDevolucion(Inicio.AddDays(1));
        Assert.Throws<ValidacionException>(() => prestamo.RegistrarDevolucion(Inicio.AddDays(2)));
    }
}

public class MultaTests
{
    [Theory]
    [InlineData(-3, 0)]
    [InlineData(0, 0)]
    [InlineData(1, 2)]
    [InlineData(5, 10)]
    [InlineData(50, 100)]
    [InlineData(500, 100)]
    public void MultaPorDia_cobra_tarifa_diaria_con_tope(int dias, decimal esperado)
    {
        Assert.Equal(esperado, new MultaPorDia().Calcular(dias));
    }

    [Fact]
    public void Pagar_marca_la_multa_y_no_se_puede_pagar_dos_veces()
    {
        var multa = new Multa(1, 1, 3, 6m, new DateTime(2026, 9, 1));
        multa.Pagar(new DateTime(2026, 9, 2));
        Assert.True(multa.Pagada);
        Assert.Equal(new DateTime(2026, 9, 2), multa.FechaPago);
        Assert.Throws<ValidacionException>(() => multa.Pagar(new DateTime(2026, 9, 3)));
    }

    [Fact]
    public void Constructor_rechaza_multas_sin_atraso_o_sin_monto()
    {
        Assert.Throws<ValidacionException>(() => new Multa(1, 1, 0, 5m, DateTime.Today));
        Assert.Throws<ValidacionException>(() => new Multa(1, 1, 2, 0m, DateTime.Today));
    }
}
