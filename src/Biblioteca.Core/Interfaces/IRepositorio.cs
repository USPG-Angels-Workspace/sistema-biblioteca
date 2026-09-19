using Biblioteca.Core.Modelos;

namespace Biblioteca.Core.Interfaces;

/// <summary>Acceso a una colección persistente de entidades.</summary>
public interface IRepositorio<T> where T : EntidadBase
{
    IReadOnlyList<T> ObtenerTodos();
    T? ObtenerPorId(int id);
    void Agregar(T entidad);
    void Actualizar(T entidad);
    void Eliminar(int id);
}
