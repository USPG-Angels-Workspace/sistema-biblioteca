using System.Net;
using System.Text.RegularExpressions;
using Biblioteca.Core;
using Biblioteca.Core.Modelos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Biblioteca.Tests;

/// <summary>Aplicacion web completa sobre una copia temporal de los datos de prueba y con reloj fijo.</summary>
public sealed class AppWeb : WebApplicationFactory<Program>
{
    public string Carpeta { get; } = Path.Combine(Path.GetTempPath(), "biblioteca-web-" + Guid.NewGuid().ToString("N"));
    public RelojFalso Reloj { get; } = new() { Hoy = new DateTime(2026, 9, 19) };

    public AppWeb()
    {
        Directory.CreateDirectory(Carpeta);
        foreach (var archivo in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Data"), "*.json"))
            File.Copy(archivo, Path.Combine(Carpeta, Path.GetFileName(archivo)));
    }

    public SistemaBiblioteca Sistema => Services.GetRequiredService<SistemaBiblioteca>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(servicios =>
        {
            servicios.RemoveAll<SistemaBiblioteca>();
            servicios.AddSingleton(new SistemaBiblioteca(Carpeta, Reloj));
        });
    }

    public HttpClient Cliente() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && Directory.Exists(Carpeta))
            Directory.Delete(Carpeta, recursive: true);
    }
}

public class WebTests : IDisposable
{
    private readonly AppWeb _app = new();
    private readonly HttpClient _cliente;

    public WebTests() => _cliente = _app.Cliente();

    public void Dispose()
    {
        _cliente.Dispose();
        _app.Dispose();
    }

    /// <summary>Abre el formulario (para obtener el token antiforgery) y lo envia con los campos indicados.</summary>
    private async Task<HttpResponseMessage> Enviar(string paginaFormulario, Dictionary<string, string> campos, string? destino = null)
    {
        var html = await _cliente.GetStringAsync(paginaFormulario);
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        Assert.False(string.IsNullOrEmpty(token), "El formulario debe incluir el token antiforgery.");
        campos["__RequestVerificationToken"] = token;
        return await _cliente.PostAsync(destino ?? paginaFormulario, new FormUrlEncodedContent(campos));
    }

    private async Task<string> Texto(string ruta)
    {
        var respuesta = await _cliente.GetAsync(ruta);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        return WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/", "Próximos vencimientos")]
    [InlineData("/Libros", "Breve historia del tiempo")]
    [InlineData("/Libros/Crear", "Nuevo libro")]
    [InlineData("/Libros/Editar/1", "Editar libro")]
    [InlineData("/Usuarios", "María Fernanda López")]
    [InlineData("/Usuarios/Crear", "Nuevo usuario")]
    [InlineData("/Prestamos", "Luis Eduardo Morales")]
    [InlineData("/Prestamos/Nuevo", "Nuevo préstamo")]
    [InlineData("/Devoluciones", "Registrar devolución")]
    [InlineData("/Multas", "Sapiens")]
    [InlineData("/Reportes", "Libros disponibles")]
    [InlineData("/Reportes/Index/atrasados", "Multa estimada")]
    public async Task Las_paginas_cargan_con_los_datos_de_prueba(string ruta, string esperado)
    {
        Assert.Contains(esperado, await Texto(ruta));
    }

    [Fact]
    public async Task Un_libro_inexistente_devuelve_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.GetAsync("/Libros/Editar/9999")).StatusCode);
    }

    [Fact]
    public async Task La_busqueda_de_libros_filtra_por_texto_y_categoria()
    {
        var html = await Texto("/Libros?q=quijote&categoria=Ficcion");
        Assert.Contains("Don Quijote", html);
        Assert.DoesNotContain("Clean Code", html);
    }

    [Fact]
    public async Task Registrar_un_libro_valido_lo_guarda_en_el_archivo_json_y_muestra_confirmacion()
    {
        var respuesta = await Enviar("/Libros/Crear", new()
        {
            ["Isbn"] = "978-9929-00-099-9", ["Titulo"] = "Libro web", ["Autor"] = "Autor", ["Editorial"] = "Editorial",
            ["Anio"] = "2020", ["Categoria"] = "Tecnologia", ["Copias"] = "2"
        });

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Contains("se registró correctamente", await Texto("/Libros"));
        Assert.Contains("Libro web", File.ReadAllText(Path.Combine(_app.Carpeta, "libros.json")));
    }

    [Fact]
    public async Task Un_libro_con_isbn_invalido_vuelve_al_formulario_con_el_error_y_sin_guardar()
    {
        var respuesta = await Enviar("/Libros/Crear", new()
        {
            ["Isbn"] = "123", ["Titulo"] = "No se guarda", ["Autor"] = "A", ["Editorial"] = "E",
            ["Anio"] = "2020", ["Categoria"] = "Otros", ["Copias"] = "1"
        });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("El ISBN debe tener 10 o 13 dígitos", WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync()));
        Assert.DoesNotContain("No se guarda", File.ReadAllText(Path.Combine(_app.Carpeta, "libros.json")));
    }

    [Fact]
    public async Task Los_campos_obligatorios_vacios_se_validan()
    {
        var respuesta = await Enviar("/Libros/Crear", new() { ["Isbn"] = "", ["Titulo"] = "", ["Categoria"] = "Otros" });
        var html = WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync());
        Assert.Contains("El título es obligatorio", html);
        Assert.Contains("El año de publicación es obligatorio", html);
    }

    [Fact]
    public async Task No_se_puede_eliminar_un_libro_con_historial_pero_si_uno_sin_historial()
    {
        var conHistorial = _app.Sistema.Libros.Listar().First(l => l.Titulo == "Rayuela");
        var sinHistorial = _app.Sistema.Libros.Listar().First(l => l.Titulo == "Popol Vuh");

        await Enviar("/Libros", new(), $"/Libros/Eliminar/{conHistorial.Id}");
        Assert.Contains("historial", await Texto("/Libros"));
        Assert.NotNull(_app.Sistema.Libros.ObtenerPorId(conHistorial.Id));

        await Enviar("/Libros", new(), $"/Libros/Eliminar/{sinHistorial.Id}");
        Assert.Null(_app.Sistema.Libros.ObtenerPorId(sinHistorial.Id));
    }

    [Fact]
    public async Task Un_usuario_con_dpi_invalido_se_rechaza_y_uno_valido_se_registra()
    {
        var campos = () => new Dictionary<string, string>
        {
            ["Tipo"] = "Lector", ["Nombre"] = "Persona Web", ["Documento"] = "1234", ["Telefono"] = "55550000",
            ["Correo"] = "web@ejemplo.com", ["Detalle"] = "2026-0999", ["Activo"] = "true"
        };
        var rechazado = await Enviar("/Usuarios/Crear", campos());
        Assert.Contains("13 dígitos", WebUtility.HtmlDecode(await rechazado.Content.ReadAsStringAsync()));

        var valido = campos();
        valido["Documento"] = "2456789099101";
        Assert.Equal(HttpStatusCode.Redirect, (await Enviar("/Usuarios/Crear", valido)).StatusCode);
        Assert.Contains(_app.Sistema.Usuarios.Listar(), u => u.Nombre == "Persona Web" && u is Lector);
    }

    [Fact]
    public async Task Un_prestamo_a_un_usuario_con_libros_atrasados_se_rechaza()
    {
        var luis = _app.Sistema.Usuarios.Listar().First(u => u.Nombre.StartsWith("Luis"));
        var libro = _app.Sistema.Libros.Listar().First(l => l.Titulo == "C# in Depth");

        var respuesta = await Enviar("/Prestamos/Nuevo", new() { ["UsuarioId"] = luis.Id.ToString(), ["LibroId"] = libro.Id.ToString() });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("atrasados", WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task Prestar_renovar_y_devolver_a_tiempo_actualizan_los_ejemplares_y_los_archivos()
    {
        var ana = _app.Sistema.Usuarios.Listar().First(u => u.Nombre.StartsWith("Ana"));
        var libro = _app.Sistema.Libros.Listar().First(l => l.Titulo == "C# in Depth");

        Assert.Equal(HttpStatusCode.Redirect,
            (await Enviar("/Prestamos/Nuevo", new() { ["UsuarioId"] = ana.Id.ToString(), ["LibroId"] = libro.Id.ToString() })).StatusCode);
        Assert.Equal(1, _app.Sistema.Libros.ObtenerPorId(libro.Id)!.CopiasDisponibles);
        var prestamo = _app.Sistema.Prestamos.Listar(soloPendientes: true).First(p => p.Usuario == ana.Nombre);

        await Enviar("/Prestamos", new(), $"/Prestamos/Renovar/{prestamo.Id}");
        Assert.Contains("Préstamo renovado", await Texto("/Prestamos"));

        await Enviar("/Devoluciones", new(), $"/Devoluciones/Registrar/{prestamo.Id}");
        Assert.Contains("fue devuelto", await Texto("/Devoluciones"));
        Assert.Equal(2, _app.Sistema.Libros.ObtenerPorId(libro.Id)!.CopiasDisponibles);
        Assert.Contains("\"fechaDevolucion\": \"2026-09-19", File.ReadAllText(Path.Combine(_app.Carpeta, "prestamos.json")));
    }

    [Fact]
    public async Task Devolver_con_atraso_genera_la_multa_y_se_puede_pagar_desde_la_web()
    {
        var atrasado = _app.Sistema.Prestamos.Listar(soloPendientes: true).First(p => p.Libro == "Breve historia del tiempo");

        await Enviar("/Devoluciones", new(), $"/Devoluciones/Registrar/{atrasado.Id}");
        Assert.Contains("Se generó una multa de Q14.00", await Texto("/Devoluciones"));

        var multa = _app.Sistema.Multas.Listar(pagadas: false).First(m => m.Usuario.StartsWith("Luis"));
        await Enviar("/Multas", new(), $"/Multas/Pagar/{multa.Id}");
        Assert.Contains("Se registró el pago", await Texto("/Multas?estado=pendientes"));
        Assert.Contains(_app.Sistema.Multas.Listar(pagadas: true), m => m.Id == multa.Id);
    }

    [Fact]
    public async Task Los_reportes_se_exportan_a_csv_con_bom_y_encabezados()
    {
        var respuesta = await _cliente.GetAsync("/Reportes/Exportar/multas");
        Assert.Equal("text/csv", respuesta.Content.Headers.ContentType!.MediaType);
        var bytes = await respuesta.Content.ReadAsByteArrayAsync();
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3).ToArray());
        Assert.Contains("Usuario,Libro,Días de atraso,Monto,Estado,Fecha de pago", System.Text.Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public async Task Un_envio_sin_token_antiforgery_se_rechaza()
    {
        var respuesta = await _cliente.PostAsync("/Libros/Eliminar/1", new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
