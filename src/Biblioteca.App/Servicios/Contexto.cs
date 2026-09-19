namespace Biblioteca.App.Servicios;

public static class Contexto
{
    /// <summary>Carpeta con los archivos .json; se puede cambiar con la variable de entorno BIBLIOTECA_DATOS.</summary>
    public static string CarpetaDatos()
    {
        var personalizada = Environment.GetEnvironmentVariable("BIBLIOTECA_DATOS");
        return string.IsNullOrWhiteSpace(personalizada)
            ? Path.Combine(AppContext.BaseDirectory, "datos")
            : personalizada;
    }
}
