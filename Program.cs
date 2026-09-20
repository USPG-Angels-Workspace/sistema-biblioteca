// Punto de entrada de la aplicacion web (ASP.NET Core MVC).
// Arquitectura: Modelo = Biblioteca.Core (dominio, servicios y archivos JSON),
// Vista = Views/*.cshtml (Razor), Controlador = Controllers/*.cs.

using Biblioteca.Core;

// Cultura de Guatemala: punto decimal (Q14.00) y fechas en espanol.
var cultura = new System.Globalization.CultureInfo("es-GT");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultura;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebApplication.CreateBuilder(args);

// Carpeta con los archivos .json (actua como "base de datos"). Se puede cambiar con la
// configuracion "DataPath" o la variable de entorno BIBLIOTECA_DATOS; por defecto es Data/.
var carpetaDatos = builder.Configuration["DataPath"]
    ?? Environment.GetEnvironmentVariable("BIBLIOTECA_DATOS")
    ?? Path.Combine(builder.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(carpetaDatos);

// Un solo SistemaBiblioteca para toda la aplicacion: carga los JSON en memoria una vez y
// cada cambio se vuelve a escribir en disco.
builder.Services.AddSingleton(new SistemaBiblioteca(carpetaDatos));

// Habilita el patron MVC con mensajes de validacion en espanol.
builder.Services.AddControllersWithViews(opciones =>
{
    var mensajes = opciones.ModelBindingMessageProvider;
    mensajes.SetValueMustBeANumberAccessor(campo => $"El campo «{campo}» debe ser un número.");
    mensajes.SetValueIsInvalidAccessor(valor => $"El valor «{valor}» no es válido.");
    mensajes.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"El valor «{valor}» no es válido para «{campo}».");
    mensajes.SetMissingBindRequiredValueAccessor(campo => $"Falta el valor de «{campo}».");
    mensajes.SetNonPropertyValueMustBeANumberAccessor(() => "El valor debe ser un número.");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// Los datos viven en memoria y en archivos: se atiende una peticion a la vez para que
// dos usuarios no modifiquen los mismos archivos al mismo tiempo.
var turno = new SemaphoreSlim(1, 1);
app.Use(async (contexto, siguiente) =>
{
    await turno.WaitAsync();
    try { await siguiente(contexto); }
    finally { turno.Release(); }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Permite que las pruebas de integracion arranquen la aplicacion completa.
public partial class Program { }
