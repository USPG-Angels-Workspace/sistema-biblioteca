using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Biblioteca.Core.Excepciones;
using Biblioteca.Core.Interfaces;
using Biblioteca.Core.Modelos;

namespace Biblioteca.Core.Persistencia;

/// <summary>
/// Repositorio que mantiene la colección en memoria y la respalda en un archivo .json.
/// Cada cambio se escribe primero en un archivo temporal y luego reemplaza al original,
/// así un fallo a medio guardado no deja el archivo de datos corrupto.
/// </summary>
public class RepositorioJson<T> : IRepositorio<T> where T : EntidadBase
{
    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _ruta;
    private readonly List<T> _elementos;

    public RepositorioJson(string ruta)
    {
        _ruta = ruta;
        _elementos = Cargar();
    }

    public IReadOnlyList<T> ObtenerTodos() => _elementos.ToList();

    public T? ObtenerPorId(int id) => _elementos.FirstOrDefault(e => e.Id == id);

    public void Agregar(T entidad)
    {
        entidad.AsignarId(_elementos.Count == 0 ? 1 : _elementos.Max(e => e.Id) + 1);
        _elementos.Add(entidad);
        Guardar();
    }

    public void Actualizar(T entidad)
    {
        if (!_elementos.Any(e => e.Id == entidad.Id))
            throw new ValidacionException("El registro que se intenta actualizar no existe.");
        Guardar();
    }

    public void Eliminar(int id)
    {
        var existente = ObtenerPorId(id)
            ?? throw new ValidacionException("El registro que se intenta eliminar no existe.");
        _elementos.Remove(existente);
        Guardar();
    }

    private List<T> Cargar()
    {
        try
        {
            if (!File.Exists(_ruta))
                return new List<T>();

            var contenido = File.ReadAllText(_ruta);
            if (string.IsNullOrWhiteSpace(contenido))
                return new List<T>();

            return JsonSerializer.Deserialize<List<T>>(contenido, Opciones) ?? new List<T>();
        }
        catch (JsonException ex)
        {
            throw new AlmacenamientoException(
                $"El archivo «{Path.GetFileName(_ruta)}» está dañado o tiene un formato inválido.", ex);
        }
        catch (IOException ex)
        {
            throw new AlmacenamientoException(
                $"No se pudo leer el archivo «{Path.GetFileName(_ruta)}».", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new AlmacenamientoException(
                $"No hay permiso para leer el archivo «{Path.GetFileName(_ruta)}».", ex);
        }
    }

    private void Guardar()
    {
        var temporal = _ruta + ".tmp";
        try
        {
            var carpeta = Path.GetDirectoryName(_ruta);
            if (!string.IsNullOrEmpty(carpeta))
                Directory.CreateDirectory(carpeta);

            File.WriteAllText(temporal, JsonSerializer.Serialize(_elementos, Opciones));
            File.Move(temporal, _ruta, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new AlmacenamientoException(
                $"No se pudo guardar el archivo «{Path.GetFileName(_ruta)}».", ex);
        }
    }
}
