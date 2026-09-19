namespace Biblioteca.Core.Interfaces;

/// <summary>Origen de la fecha actual; permite probar vencimientos sin depender del calendario real.</summary>
public interface IReloj
{
    DateTime Hoy { get; }
}
