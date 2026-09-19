using System.Text.Json.Serialization;

namespace Biblioteca.Core.Modelos;

/// <summary>Clase base de todo objeto que se guarda en un archivo JSON: aporta el identificador.</summary>
public abstract class EntidadBase
{
    [JsonInclude]
    public int Id { get; private set; }

    internal void AsignarId(int id) => Id = id;
}
